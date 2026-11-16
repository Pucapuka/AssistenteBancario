using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

using AssistenteBancario.API.DTOs;
using AssistenteBancario.API.Tools;

namespace AssistenteBancario.API.AI;

public class AssistenteService
{
    private readonly OpenRouterClient _openRouter;
    private readonly ConsultarSaldoTool _consultarSaldo;
    private readonly DepositarSaldoTool _depositar;
    private readonly TransferirSaldoTool _transferir;
    private readonly OperacaoPendenteStore _operacoesPendentes;

    public AssistenteService(
        OpenRouterClient openRouter,
        ConsultarSaldoTool consultarSaldo,
        DepositarSaldoTool depositar,
        TransferirSaldoTool transferir,
        OperacaoPendenteStore operacoesPendentes)
    {
        _openRouter = openRouter;
        _consultarSaldo = consultarSaldo;
        _depositar = depositar;
        _transferir = transferir;
        _operacoesPendentes = operacoesPendentes;
    }

    private static string CriarMensagemDeterministica(
    ResultadoFerramenta resultado)
    {
        // Existe uma operação aguardando confirmação.
        if (resultado.OperacaoPendente is { } operacao)
        {
            var valor = operacao.Valor.ToString(
                "C2",
                CultureInfo.GetCultureInfo("pt-BR")
            );
            if (operacao.Tipo == "Deposito")
            {
                return
                    $"Sua solicitação de depósito foi registrada.\n\n" +
                    $"**Conta:** {operacao.NumeroContaOrigem}\n" +
                    $"**Valor:** {valor}\n" +
                    $"**Identificador:** `{operacao.Id}`\n\n" +
                    "O depósito ainda não foi executado. " +
                    "Aguarde a confirmação explícita para que " +
                    "o backend realize a operação.";
            }
            if (operacao.Tipo == "Transferencia")
            {
                return
                    $"Sua solicitação de transferência foi registrada.\n\n" +
                    $"**Conta de origem:** {operacao.NumeroContaOrigem}\n" +
                    $"**Conta de destino:** {operacao.NumeroContaDestino}\n" +
                    $"**Valor:** {valor}\n" +
                    $"**Identificador:** `{operacao.Id}`\n\n" +
                    "A transferência ainda não foi executada. " +
                    "Aguarde a confirmação explícita para que " +
                    "o backend realize a operação.";
            }
        }
        // A operação foi recusada pela validação do backend.
        if (resultado.Resultado is OperacaoBancariaResponse resposta)
        {
            return resposta.Mensagem;
        }
        return "Não foi possível processar a operação bancária.";
    }

    public async Task<AssistenteChatResponse> ConversarAsync(
        int numeroConta,
        string mensagem,
        CancellationToken cancellationToken = default)
    {
        if (numeroConta <= 0)
            throw new ArgumentException("Número de conta inválido.");

        if (string.IsNullOrWhiteSpace(mensagem))
            throw new ArgumentException("A mensagem não pode estar vazia.");

        if (mensagem.Length > 2000)
            throw new ArgumentException("A mensagem excede o limite permitido.");

        var mensagens = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "system",
                ["content"] =
                    SystemPrompt.Conteudo +
                    $"\n\nA conta ativa desta conversa é {numeroConta}. " +
                    "Não utilize outra conta como origem ou destino " +
                    "sem que isso seja permitido pelas regras da operação."
            },
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = mensagem
            }
        };

        var requisicao = new JsonObject
        {
            ["model"] = "openrouter/free",
            ["messages"] = mensagens,
            ["tools"] = ToolDefinitions.Criar(),
            ["tool_choice"] = "auto"
        };

        using var resposta = await _openRouter.EnviarChatAsync(
            requisicao,
            cancellationToken
        );

        var mensagemModelo = ObterMensagemModelo(resposta);

        if (!mensagemModelo.TryGetProperty(
                "tool_calls",
                out var chamadas) ||
            chamadas.ValueKind != JsonValueKind.Array ||
            chamadas.GetArrayLength() == 0)
        {
            return new AssistenteChatResponse(
                ObterTexto(mensagemModelo),
                null
            );
        }

        // Por segurança, aceitamos somente uma ferramenta por interação.
        // Nenhuma operação é executada quando o modelo solicita várias.
        if (chamadas.GetArrayLength() != 1)
        {
            return new AssistenteChatResponse(
                "Não foi possível processar a solicitação com segurança. " +
                "Tente solicitar uma operação por vez.",
                null
            );
        }

        var chamada = chamadas[0];

        var chamadaId = chamada
            .GetProperty("id")
            .GetString();

        var funcao = chamada.GetProperty("function");

        var nomeFerramenta = funcao
            .GetProperty("name")
            .GetString();

        var argumentosJson = funcao
            .GetProperty("arguments")
            .GetString();

        if (string.IsNullOrWhiteSpace(chamadaId) ||
            string.IsNullOrWhiteSpace(nomeFerramenta) ||
            string.IsNullOrWhiteSpace(argumentosJson))
        {
            return new AssistenteChatResponse(
                "Não consegui interpretar a solicitação. " +
                "Por favor, tente novamente.",
                null
            );
        }

        using var argumentos = JsonDocument.Parse(argumentosJson);

        var resultado = await ExecutarFerramentaAsync(
            nomeFerramenta,
            argumentos.RootElement,
            numeroConta,
            cancellationToken
        );

        // Operações bancárias de escrita possuem respostas determinísticas.
        // Não solicitamos ao modelo que reformule o estado financeiro.
        if (resultado.RespostaDeterministica)
        {
            return new AssistenteChatResponse(
                CriarMensagemDeterministica(resultado),
                resultado.OperacaoPendente
            );
        }

        // Guardamos a mensagem original do modelo, incluindo tool_calls.
        mensagens.Add(
            JsonNode.Parse(mensagemModelo.GetRawText())
        );

        // Retornamos o resultado da ferramenta ao modelo.
        mensagens.Add(
            new JsonObject
            {
                ["role"] = "tool",
                ["tool_call_id"] = chamadaId,
                ["content"] = JsonSerializer.Serialize(resultado.Resultado)
            }
        );

        // Segunda chamada: o modelo apenas formula a resposta.
        // Não permitimos novas chamadas de ferramentas nesta etapa.
        var requisicaoFinal = new JsonObject
        {
            ["model"] = "openrouter/free",
            ["messages"] = mensagens.DeepClone(),
            ["tool_choice"] = "none"
        };

        using var respostaFinal = await _openRouter.EnviarChatAsync(
            requisicaoFinal,
            cancellationToken
        );

        var mensagemFinal = ObterMensagemModelo(respostaFinal);

        return new AssistenteChatResponse(
            ObterTexto(mensagemFinal),
            resultado.OperacaoPendente
        );
    }

    private async Task<ResultadoFerramenta> ExecutarFerramentaAsync(
        string nome,
        JsonElement argumentos,
        int numeroContaAtiva,
        CancellationToken cancellationToken)
    {
        switch (nome)
        {
            case "consultar_saldo":
            {
                var numeroConta = LerInteiro(
                    argumentos,
                    "numeroConta"
                );

                ValidarContaAtiva(numeroConta, numeroContaAtiva);

                var resposta = await _consultarSaldo.ExecutarAsync(
                    numeroConta
                );

                return new ResultadoFerramenta(
                    resposta,
                    null
                );
            }

            case "depositar":
            {
                var numeroConta = LerInteiro(
                    argumentos,
                    "numeroConta"
                );

                var valor = LerDecimal(
                    argumentos,
                    "valor"
                );

                ValidarContaAtiva(numeroConta, numeroContaAtiva);
                ValidarValor(valor);

                // Não executa o depósito.
                // Apenas registra a operação para confirmação.
                var pendente = _operacoesPendentes.Criar(
                    TipoOperacaoPendente.Deposito,
                    numeroConta,
                    null,
                    valor
                );

                var dados = CriarRespostaPendente(pendente);

                return new ResultadoFerramenta(
                    new
                    {
                        sucesso = true,
                        aguardandoConfirmacao = true,
                        operacao = dados,
                        mensagem =
                            "Depósito aguardando confirmação explícita do usuário."
                    },
                    dados,
                    true
                );
            }

            case "transferir":
            {
                var numeroContaOrigem = LerInteiro(
                    argumentos,
                    "numeroContaOrigem"
                );

                var numeroContaDestino = LerInteiro(
                    argumentos,
                    "numeroContaDestino"
                );

                var valor = LerDecimal(
                    argumentos,
                    "valor"
                );

                ValidarContaAtiva(
                    numeroContaOrigem,
                    numeroContaAtiva
                );

                ValidarValor(valor);

                if (numeroContaDestino <= 0)
                    throw new ArgumentException(
                        "Número da conta de destino inválido."
                    );

                if (numeroContaOrigem == numeroContaDestino)
                    throw new ArgumentException(
                        "A conta de origem e destino não podem ser iguais."
                    );
                // Pré-valida a transferência antes de criar a pendência.
                var validacao = await _transferir.ValidarAsync(
                    numeroContaOrigem,
                    numeroContaDestino,
                    valor
                );
                
                // Se a transferência não for válida,
                // devolvemos o resultado sem criar uma operação pendente.
                if (!validacao.Sucesso)
                {
                    return new ResultadoFerramenta(
                        validacao,
                        null,
                        true
                    );
                }

                // Não executa a transferência.
                // Apenas registra a operação para confirmação.
                var pendente = _operacoesPendentes.Criar(
                    TipoOperacaoPendente.Transferencia,
                    numeroContaOrigem,
                    numeroContaDestino,
                    valor
                );

                var dados = CriarRespostaPendente(pendente);

                return new ResultadoFerramenta(
                    new
                    {
                        sucesso = true,
                        aguardandoConfirmacao = true,
                        operacao = dados,
                        mensagem =
                            "Transferência aguardando confirmação explícita do usuário."
                    },
                    dados,
                    true
                );
            }

            default:
                throw new InvalidOperationException(
                    "Ferramenta não autorizada."
                );
        }
    }

    public async Task<OperacaoBancariaResponse> ConfirmarOperacaoAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!_operacoesPendentes.TentarConsumir(id, out var operacao) ||
            operacao is null)
        {
            return new OperacaoBancariaResponse(
                false,
                "Operação inexistente, expirada ou já confirmada.",
                null
            );
        }

        switch (operacao.Tipo)
        {
            case TipoOperacaoPendente.Deposito:
                return await _depositar.ExecutarAsync(
                    operacao.NumeroContaOrigem,
                    operacao.Valor
                );

            case TipoOperacaoPendente.Transferencia:
                if (operacao.NumeroContaDestino is null)
                {
                    return new OperacaoBancariaResponse(
                        false,
                        "Conta de destino não informada.",
                        null
                    );
                }

                return await _transferir.ExecutarAsync(
                    operacao.NumeroContaOrigem,
                    operacao.NumeroContaDestino.Value,
                    operacao.Valor
                );

            default:
                return new OperacaoBancariaResponse(
                    false,
                    "Tipo de operação inválido.",
                    null
                );
        }
    }

    private static void ValidarContaAtiva(
        int numeroContaSolicitada,
        int numeroContaAtiva)
    {
        if (numeroContaSolicitada != numeroContaAtiva)
        {
            throw new ArgumentException(
                "A operação não corresponde à conta ativa."
            );
        }
    }

    private static void ValidarValor(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException(
                "O valor deve ser maior que zero."
            );

        if (decimal.Round(valor, 2) != valor)
            throw new ArgumentException(
                "O valor deve possuir no máximo duas casas decimais."
            );
    }

    private static int LerInteiro(
        JsonElement argumentos,
        string propriedade)
    {
        if (!argumentos.TryGetProperty(
                propriedade,
                out var elemento) ||
            !elemento.TryGetInt32(out var valor))
        {
            throw new ArgumentException(
                $"Parâmetro inválido: {propriedade}."
            );
        }

        return valor;
    }

    private static decimal LerDecimal(
        JsonElement argumentos,
        string propriedade)
    {
        if (!argumentos.TryGetProperty(
                propriedade,
                out var elemento) ||
            !elemento.TryGetDecimal(out var valor))
        {
            throw new ArgumentException(
                $"Parâmetro inválido: {propriedade}."
            );
        }

        return valor;
    }

    private static OperacaoPendenteResponse CriarRespostaPendente(
        OperacaoPendente operacao)
    {
        return new OperacaoPendenteResponse(
            operacao.Id,
            operacao.Tipo.ToString(),
            operacao.NumeroContaOrigem,
            operacao.NumeroContaDestino,
            operacao.Valor,
            operacao.ExpiraEm
        );
    }

    private static JsonElement ObterMensagemModelo(
        JsonDocument resposta)
    {
        var raiz = resposta.RootElement;

        if (!raiz.TryGetProperty("choices", out var escolhas) ||
            escolhas.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "O modelo não retornou nenhuma resposta."
            );
        }

        return escolhas[0].GetProperty("message");
    }

    private static string ObterTexto(JsonElement mensagem)
    {
        if (mensagem.TryGetProperty("content", out var conteudo) &&
            conteudo.ValueKind == JsonValueKind.String)
        {
            return conteudo.GetString() ?? string.Empty;
        }

        return "Não foi possível gerar uma resposta.";
    }

    private sealed record ResultadoFerramenta(
        object Resultado,
        OperacaoPendenteResponse? OperacaoPendente,
        bool RespostaDeterministica = false
    );
}