using AssistenteBancario.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssistenteBancario.API.Persistence;

public class BancoInitializer
{
    private readonly BancoDbContext _context;

    public BancoInitializer(BancoDbContext context)
    {
        _context = context;
    }

    public async Task InicializarAsync()
    {
        await _context.Database.MigrateAsync();

        // Cria a primeira conta somente se ela não existir.
        if (!await _context.Contas.AnyAsync(c => c.Numero == 1001))
        {
            var cliente = new ClienteEntity
            {
                Id = await ObterProximoClienteIdAsync(),
                Nome = "Cliente de Demonstração",
                DataNascimento = new DateOnly(1990, 1, 1),
                Cpf = "00000000000",
                Telefone = "00000000000",
                Logradouro = "Endereço demonstrativo",
                Numero = 0,
                Complemento = "Não informado",
                Bairro = "Centro",
                Cidade = "Cidade demonstrativa",
                Estado = "MA",
                Cep = "00000-000",
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
            _context.Contas.Add(conta);
        }

        // Cria a segunda conta somente se ela não existir.
        if (!await _context.Contas.AnyAsync(c => c.Numero == 1002))
        {
            var cliente = new ClienteEntity
            {
                Id = await ObterProximoClienteIdAsync(),
                Nome = "Segundo Cliente de Demonstração",
                DataNascimento = new DateOnly(1992, 5, 15),
                Cpf = "11111111111",
                Telefone = "11111111111",
                Logradouro = "Endereço demonstrativo",
                Numero = 100,
                Complemento = "Não informado",
                Bairro = "Centro",
                Cidade = "Cidade demonstrativa",
                Estado = "MA",
                Cep = "00000-001",
                Pais = "Brasil",
                Zona = "Urbana"
            };

            var conta = new ContaEntity
            {
                Numero = 1002,
                ClienteId = cliente.Id,
                SaldoCentavos = 50000,
                Cliente = cliente
            };

            cliente.Conta = conta;

            _context.Clientes.Add(cliente);
            _context.Contas.Add(conta);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<int> ObterProximoClienteIdAsync()
    {
        return (await _context.Clientes
            .MaxAsync(c => (int?)c.Id) ?? 0) + 1;
    }
}