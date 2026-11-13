using AssistenteBancario.API.DTOs;
using AssistenteBancario.API.Services;

namespace AssistenteBancario.API.Tools;

public class ConsultarSaldoTool
{
    private readonly BancoOperacoesService _bancoService;

    public ConsultarSaldoTool(BancoOperacoesService bancoService)
    {
        _bancoService = bancoService;
    }

    public Task<OperacaoBancariaResponse> ExecutarAsync(
        int numeroConta)
    {
        return _bancoService.ConsultarSaldoAsync(numeroConta);
    }
}