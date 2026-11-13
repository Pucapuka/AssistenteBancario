namespace AssistenteBancario.API.DTOs;

public record OperacaoBancariaResponse(
    bool Sucesso,
    string Mensagem,
    decimal? SaldoAtual
);