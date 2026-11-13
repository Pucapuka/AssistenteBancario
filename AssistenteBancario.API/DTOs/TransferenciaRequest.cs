namespace AssistenteBancario.API.DTOs;

public record TransferenciaRequest(
    int NumeroContaDestino,
    decimal Valor
);