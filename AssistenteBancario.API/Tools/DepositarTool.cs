using AssistenteBancario.API.DTOs;
using AssistenteBancario.API.Services;

namespace AssistenteBancario.API.Tools;

public class DepositarSaldoTool
{
    private readonly BancoOperacoesService _bancoService;

    public DepositarSaldoTool(BancoOperacoesService bancoService)
    {
        _bancoService = bancoService;
    }

    public Task<OperacaoBancariaResponse> ExecutarAsync(
        int numeroConta,
        decimal valor)
    {
        return _bancoService.DepositarAsync(numeroConta, valor);
    }
}