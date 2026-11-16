using AssistenteBancario.API.Persistence;
using AssistenteBancario.API.Services;
using Microsoft.EntityFrameworkCore;
using AssistenteBancario.API.Tools;
using System.Text.Json.Nodes;
using AssistenteBancario.API.AI;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco SQLite
var databaseDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    "Data"
);

Directory.CreateDirectory(databaseDirectory);

var databasePath = Path.Combine(
    databaseDirectory,
    "assistente-bancario.db"
);

builder.Services.AddDbContext<BancoDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

// Serviços da aplicação
builder.Services.AddScoped<BancoInitializer>();

//Serviços de Ferramentas - consulta de saldo
builder.Services.AddScoped<ConsultarSaldoTool>();

//Serviços de Ferramentas - depósito
builder.Services.AddScoped<DepositarSaldoTool>();

//Serviços de Ferramentas - transferência
builder.Services.AddScoped<TransferirSaldoTool>();

builder.Services.AddScoped<BancoOperacoesService>();

// Cliente HTTP do OpenRouter
builder.Services.AddHttpClient<OpenRouterClient>();

// OpenAPI
builder.Services.AddOpenApi();

//Controllers
builder.Services.AddControllers();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Serviços do agente de IA
builder.Services.AddSingleton<OperacaoPendenteStore>();
builder.Services.AddScoped<AssistenteService>();

var app = builder.Build();

// Inicialização do banco
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<BancoInitializer>();

    await initializer.InicializarAsync();
}

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

// Endpoint temporário para testar a conexão com o OpenRouter
if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/teste-openrouter", async (
        OpenRouterClient client,
        CancellationToken cancellationToken) =>
    {
        var requisicao = new JsonObject
        {
            ["model"] = "openrouter/free",
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = "Responda apenas: conexão estabelecida."
                }
            }
        };

        using var resposta = await client.EnviarChatAsync(
            requisicao,
            cancellationToken
        );

        var mensagem = resposta.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return Results.Ok(new
        {
            sucesso = true,
            resposta = mensagem
        });
    });
}

app.Run();
