using AssistenteBancario.API.Persistence;
using AssistenteBancario.API.Services;
using Microsoft.EntityFrameworkCore;
using AssistenteBancario.API.Tools;

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

builder.Services.AddScoped<BancoOperacoesService>();

// Serviços da aplicação
builder.Services.AddScoped<BancoInitializer>();

//Serviços de Ferramentas - consulta de saldo
builder.Services.AddScoped<ConsultarSaldoTool>();

// OpenAPI
builder.Services.AddOpenApi();

//Controllers
builder.Services.AddControllers();

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

app.MapControllers();

app.Run();