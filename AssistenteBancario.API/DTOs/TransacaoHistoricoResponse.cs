namespace AssistenteBancario.API.DTOs;

public record TransacaoHistoricoResponse(
    int Id,
    string Tipo,
    string Direcao,
    int NumeroContaOrigem,
    int? NumeroContaDestino,
    string? Destinatario,
    decimal Valor,
    DateTime DataHora,
    string Status
);

public record HistoricoTransacoesResponse(
    bool Sucesso,
    string Mensagem,
    List<TransacaoHistoricoResponse> Transacoes
);