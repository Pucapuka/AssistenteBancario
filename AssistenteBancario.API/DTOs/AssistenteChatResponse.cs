namespace AssistenteBancario.API.DTOs;

public record AssistenteChatResponse(
    string Mensagem,
    OperacaoPendenteResponse? OperacaoPendente
);

public record OperacaoPendenteResponse(
    string Id,
    string Tipo,
    int NumeroContaOrigem,
    int? NumeroContaDestino,
    decimal Valor,
    DateTimeOffset ExpiraEm
);