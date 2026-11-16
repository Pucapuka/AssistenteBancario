using AssistenteBancario.API.DTOs;
using AssistenteBancario.API.Services;

namespace AssistenteBancario.API.Tools;

public class TransferirSaldoTool
{
    private readonly BancoOperacoesService _bancoService;

    public TransferirSaldoTool(BancoOperacoesService bancoService)
    {
        _bancoService = bancoService;
    }

    public Task<OperacaoBancariaResponse> ValidarAsync(
        int numeroContaOrigem,
        int numeroContaDestino,
        decimal valor)
    {
        return _bancoService.ValidarTransferenciaAsync(
            numeroContaOrigem,
            numeroContaDestino,
            valor
        );
    }

    public Task<OperacaoBancariaResponse> ExecutarAsync(
        int numeroContaOrigem,
        int numeroContaDestino,
        decimal valor)
    {
        return _bancoService.TransferirAsync(
            numeroContaOrigem,
            numeroContaDestino,
            valor
        );
    }
}