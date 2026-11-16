using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AssistenteBancario.API.AI;

public class OpenRouterClient
{
    private readonly HttpClient _httpClient;

    public OpenRouterClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        var apiKey = configuration["OpenRouter:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "A chave da API do OpenRouter não foi configurada."
            );
        }

        _httpClient.BaseAddress = new Uri(
            "https://openrouter.ai/api/v1/"
        );

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        _httpClient.Timeout = TimeSpan.FromSeconds(90);
    }

    public async Task<JsonDocument> EnviarChatAsync(
        JsonObject requisicao,
        CancellationToken cancellationToken = default)
    {
        var json = requisicao.ToJsonString();

        using var conteudo = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        using var resposta = await _httpClient.PostAsync(
            "chat/completions",
            conteudo,
            cancellationToken
        );

        var corpoResposta = await resposta.Content.ReadAsStringAsync(
            cancellationToken
        );

        if (!resposta.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Erro no OpenRouter: HTTP {(int)resposta.StatusCode}. " +
                $"Detalhes: {corpoResposta}",
                null,
                resposta.StatusCode
            );
        }

        return JsonDocument.Parse(corpoResposta);
    }
}