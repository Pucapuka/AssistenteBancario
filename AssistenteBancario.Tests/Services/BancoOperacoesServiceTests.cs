using AssistenteBancario.API.Persistence;
using AssistenteBancario.API.Persistence.Entities;
using AssistenteBancario.API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AssistenteBancario.Tests.Services;

public class BancoOperacoesServiceTests : IDisposable
{
    private readonly string _databasePath;
    private readonly DbContextOptions<BancoDbContext> _options;
    private readonly BancoDbContext _context;
    private readonly BancoOperacoesService _service;

    public BancoOperacoesServiceTests()
    {
        _databasePath = Path.Combine(
            Path.GetTempPath(),
            $"BancoTeste_{Guid.NewGuid()}.db"
        );

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            DefaultTimeout = 30
        }.ToString();

        _options = new DbContextOptionsBuilder<BancoDbContext>()
            .UseSqlite(connectionString)
            .Options;

        _context = new BancoDbContext(_options);

        _context.Database.EnsureCreated();

        _service = new BancoOperacoesService(_context);

        CriarDadosIniciais();
    }

    private void CriarDadosIniciais()
    {
        var cliente = new ClienteEntity
        {
            Id = 1,
            Nome = "Cliente Teste",
            DataNascimento = new DateOnly(1990, 1, 1),
            Cpf = "12345678901",
            Telefone = "999999999",
            Logradouro = "Rua Teste",
            Numero = 100,
            Complemento = "",
            Bairro = "Centro",
            Cidade = "Imperatriz",
            Estado = "MA",
            Cep = "65900-000",
            Pais = "Brasil",
            Zona = "Urbana"
        };

        var conta = new ContaEntity
        {
            Numero = 1001,
            ClienteId = cliente.Id,
            SaldoCentavos = 100000,
            Cliente = cliente
        };

        cliente.Conta = conta;

        _context.Clientes.Add(cliente);
        _context.SaveChanges();
    }

    // ==========================================
    // CONSULTA DE SALDO
    // ==========================================

    [Fact]
    public async Task ConsultarSaldo_DeveRetornarSaldoCorreto()
    {
        var resultado = await _service.ConsultarSaldoAsync(1001);

        Assert.True(resultado.Sucesso);
        Assert.Equal(1000m, resultado.SaldoAtual);
    }

    [Fact]
    public async Task ConsultarSaldo_DeveFalharParaContaInexistente()
    {
        var resultado = await _service.ConsultarSaldoAsync(9999);

        Assert.False(resultado.Sucesso);
        Assert.Null(resultado.SaldoAtual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ConsultarSaldo_DeveFalharParaNumeroInvalido(
        int numeroConta)
    {
        var resultado = await _service.ConsultarSaldoAsync(numeroConta);

        Assert.False(resultado.Sucesso);
        Assert.Null(resultado.SaldoAtual);
    }

    // ==========================================
    // DEPÓSITO
    // ==========================================

    [Fact]
    public async Task Depositar_DeveAtualizarSaldo()
    {
        var resultado = await _service.DepositarAsync(1001, 100m);

        Assert.True(resultado.Sucesso);
        Assert.Equal(1100m, resultado.SaldoAtual);

        var conta = await _context.Contas
            .AsNoTracking()
            .FirstAsync(c => c.Numero == 1001);

        Assert.Equal(110000, conta.SaldoCentavos);
    }

    [Fact]
    public async Task Depositar_DeveRegistrarTransacao()
    {
        var resultado = await _service.DepositarAsync(1001, 100m);

        Assert.True(resultado.Sucesso);

        var transacao = await _context.Transacoes
            .AsNoTracking()
            .FirstOrDefaultAsync();

        Assert.NotNull(transacao);
        Assert.Equal(1001, transacao.ContaOrigem);
        Assert.Equal("DEPOSITO", transacao.Tipo);
        Assert.Equal(10000, transacao.ValorCentavos);
        Assert.Equal("CONCLUIDA", transacao.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(10.999)]
    public async Task Depositar_DeveRejeitarValoresInvalidos(decimal valor)
    {
        var resultado = await _service.DepositarAsync(1001, valor);

        Assert.False(resultado.Sucesso);

        var conta = await _context.Contas
            .AsNoTracking()
            .FirstAsync(c => c.Numero == 1001);

        Assert.Equal(100000, conta.SaldoCentavos);
    }

    [Fact]
    public async Task Depositar_DeveFalharParaContaInexistente()
    {
        var resultado = await _service.DepositarAsync(9999, 100m);

        Assert.False(resultado.Sucesso);

        var transacoes = await _context.Transacoes
            .AsNoTracking()
            .ToListAsync();

        Assert.Empty(transacoes);
    }

    // ==========================================
    // ATOMICIDADE
    // ==========================================

    [Fact]
    public async Task Depositar_QuandoRegistroFalhar_DeveReverterSaldo()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            CREATE TRIGGER ImpedirDeposito
            BEFORE INSERT ON Transacoes
            BEGIN
                SELECT RAISE(ABORT, 'Falha simulada no registro');
            END;
            """);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _service.DepositarAsync(1001, 100m)
        );

        var conta = await _context.Contas
            .AsNoTracking()
            .FirstAsync(c => c.Numero == 1001);

        var transacoes = await _context.Transacoes
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(100000, conta.SaldoCentavos);
        Assert.Empty(transacoes);
    }

    // ==========================================
    // CONCORRÊNCIA
    // ==========================================

    [Fact]
    public async Task Depositar_DuasOperacoesSimultaneas_DevePreservarConsistencia()
    {
        // Cada operação utiliza seu próprio DbContext.
        await using var contextA = new BancoDbContext(_options);
        await using var contextB = new BancoDbContext(_options);

        var serviceA = new BancoOperacoesService(contextA);
        var serviceB = new BancoOperacoesService(contextB);

        // Duas operações iniciadas simultaneamente.
        var operacaoA = serviceA.DepositarAsync(1001, 100m);
        var operacaoB = serviceB.DepositarAsync(1001, 200m);

        var resultados = await Task.WhenAll(
            operacaoA,
            operacaoB
        );

        // As duas operações devem ser concluídas.
        Assert.All(resultados, resultado =>
            Assert.True(resultado.Sucesso)
        );

        // Consultamos o banco usando uma nova instância.
        await using var verificacao = new BancoDbContext(_options);

        var conta = await verificacao.Contas
            .AsNoTracking()
            .FirstAsync(c => c.Numero == 1001);

        var transacoes = await verificacao.Transacoes
            .AsNoTracking()
            .Where(t => t.ContaOrigem == 1001)
            .ToListAsync();

        // Saldo inicial: 100000 centavos.
        // Depósitos: 10000 + 20000 centavos.
        Assert.Equal(130000, conta.SaldoCentavos);

        Assert.Equal(2, transacoes.Count);

        Assert.Equal(
            30000,
            transacoes.Sum(t => t.ValorCentavos)
        );
    }

    // ==========================================
    // DESCARTE DOS RECURSOS
    // ==========================================

    public void Dispose()
    {
        _context.Dispose();

        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}