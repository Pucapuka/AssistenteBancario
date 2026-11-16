using AssistenteBancario.API.AI;
using AssistenteBancario.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AssistenteBancario.API.Controllers;

[ApiController]
[Route("api/assistente")]
public class AssistenteController : ControllerBase
{
    private readonly AssistenteService _assistenteService;
    private readonly ILogger<AssistenteController> _logger;

    public AssistenteController(
        AssistenteService assistenteService,
        ILogger<AssistenteController> logger)
    {
        _assistenteService = assistenteService;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Conversar(
        [FromBody] AssistenteChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var resposta = await _assistenteService.ConversarAsync(
                request.NumeroConta,
                request.Mensagem,
                cancellationToken
            );

            return Ok(resposta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                sucesso = false,
                mensagem = ex.Message
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                sucesso = false,
                mensagem = "Não foi possível comunicar com o modelo de IA."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar a resposta do assistente."
            );
        
            return StatusCode(502, new
            {
                sucesso = false,
                mensagem = "Ocorreu um erro ao processar a resposta do assistente."
            });
        }
    }

    [HttpPost("confirmar/{id}")]
    public async Task<IActionResult> Confirmar(
        string id,
        CancellationToken cancellationToken)
    {
        var resposta = await _assistenteService.ConfirmarOperacaoAsync(
            id,
            cancellationToken
        );

        if (!resposta.Sucesso)
            return BadRequest(resposta);

        return Ok(resposta);
    }
}