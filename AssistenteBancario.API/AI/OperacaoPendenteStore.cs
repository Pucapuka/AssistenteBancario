using System.Collections.Concurrent;

namespace AssistenteBancario.API.AI;

public enum TipoOperacaoPendente
{
    Deposito,
    Transferencia
}

public record OperacaoPendente(
    string Id,
    TipoOperacaoPendente Tipo,
    int NumeroContaOrigem,
    int? NumeroContaDestino,
    decimal Valor,
    DateTimeOffset CriadaEm,
    DateTimeOffset ExpiraEm
);

public class OperacaoPendenteStore
{
    private readonly ConcurrentDictionary<string, OperacaoPendente>
        _operacoes = new();

    public OperacaoPendente Criar(
        TipoOperacaoPendente tipo,
        int numeroContaOrigem,
        int? numeroContaDestino,
        decimal valor)
    {
        var agora = DateTimeOffset.UtcNow;

        var operacao = new OperacaoPendente(
            Guid.NewGuid().ToString("N"),
            tipo,
            numeroContaOrigem,
            numeroContaDestino,
            valor,
            agora,
            agora.AddMinutes(10)
        );

        _operacoes.TryAdd(operacao.Id, operacao);

        return operacao;
    }

    public bool TentarConsumir(
        string id,
        out OperacaoPendente? operacao)
    {
        operacao = null;

        if (!_operacoes.TryRemove(id, out var encontrada))
            return false;

        if (encontrada.ExpiraEm <= DateTimeOffset.UtcNow)
            return false;

        operacao = encontrada;
        return true;
    }
}