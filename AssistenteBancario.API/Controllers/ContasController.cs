using AssistenteBancario.API.DTOs;
using AssistenteBancario.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssistenteBancario.API.Controllers;

[ApiController]
[Route("api/contas")]
public class ContasController : ControllerBase
{
    private readonly BancoOperacoesService _bancoService;

    public ContasController(BancoOperacoesService bancoService)
    {
        _bancoService = bancoService;
    }

    [HttpGet("{numeroConta:int}/saldo")]
    public async Task<IActionResult> ConsultarSaldo(int numeroConta)
    {
        var resultado = await _bancoService.ConsultarSaldoAsync(numeroConta);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("{numeroConta:int}/depositos")]
    public async Task<IActionResult> Depositar(
        int numeroConta,
        [FromBody] DepositoRequest request)
    {
        var resultado = await _bancoService.DepositarAsync(
            numeroConta,
            request.Valor
        );

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("{numeroConta:int}/saques")]
    public async Task<IActionResult> Sacar(
        int numeroConta,
        [FromBody] SaqueRequest request)
    {
        var resultado = await _bancoService.SacarAsync(
            numeroConta,
            request.Valor
        );

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("{numeroContaOrigem:int}/transferencias")]
    public async Task<IActionResult> Transferir(
        int numeroContaOrigem,
        [FromBody] TransferenciaRequest request)
    {
        var resultado = await _bancoService.TransferirAsync(
            numeroContaOrigem,
            request.NumeroContaDestino,
            request.Valor
        );

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpGet("{numeroConta:int}/transacoes")]
    public async Task<IActionResult> ConsultarTransacoes(int numeroConta)
    {
        var resultado = await _bancoService.ConsultarTransacoesAsync(
            numeroConta
        );
    
        if (!resultado.Sucesso)
            return BadRequest(resultado);
    
        return Ok(resultado);
    }
}