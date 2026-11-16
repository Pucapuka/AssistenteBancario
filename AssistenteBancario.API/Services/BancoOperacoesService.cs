using AssistenteBancario.API.DTOs;
using AssistenteBancario.API.Persistence;
using AssistenteBancario.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SimulacaoBanco.Models;

namespace AssistenteBancario.API.Services;

public class BancoOperacoesService
{
    private readonly BancoDbContext _context;

    public BancoOperacoesService(BancoDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // CONSULTAR SALDO
    // =========================================================

    public async Task<OperacaoBancariaResponse> ConsultarSaldoAsync(
        int numeroConta)
    {
        if (numeroConta <= 0)
        {
            return new(false, "Número de conta inválido.", null);
        }

        var conta = await _context.Contas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Numero == numeroConta);

        if (conta is null)
        {
            return new(false, "Conta não encontrada.", null);
        }

        return new(
            true,
            "Saldo consultado com sucesso.",
            conta.SaldoCentavos / 100m
        );
    }

    // =========================================================
    // DEPOSITAR
    // =========================================================

    public async Task<OperacaoBancariaResponse> DepositarAsync(
        int numeroConta,
        decimal valor)
    {
        if (numeroConta <= 0)
        {
            return new(false, "Número de conta inválido.", null);
        }

        if (!TentarConverterParaCentavos(valor, out long valorCentavos))
        {
            return new(
                false,
                "O valor do depósito deve ser positivo e possuir no máximo duas casas decimais.",
                null
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            await BloquearEscritaAsync(numeroConta);

            var contaEntity = await _context.Contas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Numero == numeroConta);

            if (contaEntity is null)
            {
                await transaction.RollbackAsync();

                return new(false, "Conta não encontrada.", null);
            }

            var conta = ContaBancaria.Reconstituir(
                contaEntity.Numero,
                CriarCliente(contaEntity.Cliente),
                contaEntity.SaldoCentavos / 100m
            );

            if (!conta.Depositar(valor))
            {
                await transaction.RollbackAsync();

                return new(false, "Não foi possível realizar o depósito.", null);
            }

            if (!TentarConverterParaCentavos(
                conta.Saldo,
                out long novoSaldo))
            {
                await transaction.RollbackAsync();

                return new(false, "Saldo resultante fora do limite permitido.", null);
            }

            contaEntity.SaldoCentavos = novoSaldo;

            _context.Transacoes.Add(new TransacaoEntity
            {
                ContaOrigem = numeroConta,
                ContaDestino = null,
                Tipo = "DEPOSITO",
                Destinatario = null,
                ValorCentavos = valorCentavos,
                DataHora = DateTime.UtcNow,
                Status = "CONCLUIDA"
            });

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new(
                true,
                "Depósito realizado com sucesso.",
                conta.Saldo
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // SACAR
    // =========================================================

    public async Task<OperacaoBancariaResponse> SacarAsync(
        int numeroConta,
        decimal valor)
    {
        if (numeroConta <= 0)
        {
            return new(false, "Número de conta inválido.", null);
        }

        if (!TentarConverterParaCentavos(valor, out long valorCentavos))
        {
            return new(
                false,
                "O valor do saque deve ser positivo e possuir no máximo duas casas decimais.",
                null
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            await BloquearEscritaAsync(numeroConta);

            var contaEntity = await _context.Contas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Numero == numeroConta);

            if (contaEntity is null)
            {
                await transaction.RollbackAsync();

                return new(false, "Conta não encontrada.", null);
            }

            var conta = ContaBancaria.Reconstituir(
                contaEntity.Numero,
                CriarCliente(contaEntity.Cliente),
                contaEntity.SaldoCentavos / 100m
            );

            if (!conta.Sacar(valor))
            {
                await transaction.RollbackAsync();

                return new(
                    false,
                    "Saque não autorizado. Verifique o saldo disponível e o valor.",
                    null
                );
            }

            if (!TentarConverterParaCentavos(
                conta.Saldo,
                out long novoSaldo))
            {
                await transaction.RollbackAsync();

                return new(false, "Saldo resultante fora do limite permitido.", null);
            }

            contaEntity.SaldoCentavos = novoSaldo;

            _context.Transacoes.Add(new TransacaoEntity
            {
                ContaOrigem = numeroConta,
                ContaDestino = null,
                Tipo = "SAQUE",
                Destinatario = null,
                ValorCentavos = valorCentavos,
                DataHora = DateTime.UtcNow,
                Status = "CONCLUIDA"
            });

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new(
                true,
                "Saque realizado com sucesso.",
                conta.Saldo
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    //==========================================================
    // Validar antes de transferir
    //==========================================================
        public async Task<OperacaoBancariaResponse> ValidarTransferenciaAsync(
        int numeroContaOrigem,
        int numeroContaDestino,
        decimal valor)
    {
        if (numeroContaOrigem <= 0 || numeroContaDestino <= 0)
        {
            return new(false, "Número de conta inválido.", null);
        }
    
        if (numeroContaOrigem == numeroContaDestino)
        {
            return new(
                false,
                "Não é possível transferir para a própria conta.",
                null
            );
        }
    
        if (!TentarConverterParaCentavos(valor, out long valorCentavos))
        {
            return new(
                false,
                "O valor da transferência deve ser positivo e possuir no máximo duas casas decimais.",
                null
            );
        }
    
        var contas = await _context.Contas
            .AsNoTracking()
            .Where(c =>
                c.Numero == numeroContaOrigem ||
                c.Numero == numeroContaDestino)
            .Select(c => new
            {
                c.Numero,
                c.SaldoCentavos
            })
            .ToListAsync();
    
        var origem = contas.FirstOrDefault(
            c => c.Numero == numeroContaOrigem
        );
    
        var destino = contas.FirstOrDefault(
            c => c.Numero == numeroContaDestino
        );
    
        if (origem is null)
        {
            return new(false, "Conta de origem não encontrada.", null);
        }
    
        if (destino is null)
        {
            return new(false, "Conta de destino não encontrada.", null);
        }
    
        if (origem.SaldoCentavos < valorCentavos)
        {
            return new(
                false,
                "Saldo insuficiente para realizar esta transferência.",
                origem.SaldoCentavos / 100m
            );
        }
    
        return new(
            true,
            "Transferência disponível para confirmação.",
            origem.SaldoCentavos / 100m
        );
    }

    // =========================================================
    // TRANSFERIR
    // =========================================================

    public async Task<OperacaoBancariaResponse> TransferirAsync(
        int numeroContaOrigem,
        int numeroContaDestino,
        decimal valor)
    {
        if (numeroContaOrigem <= 0 || numeroContaDestino <= 0)
        {
            return new(false, "Número de conta inválido.", null);
        }

        if (numeroContaOrigem == numeroContaDestino)
        {
            return new(
                false,
                "Não é possível transferir para a própria conta.",
                null
            );
        }

        if (!TentarConverterParaCentavos(valor, out long valorCentavos))
        {
            return new(
                false,
                "O valor da transferência deve ser positivo e possuir no máximo duas casas decimais.",
                null
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // SQLite serializa as operações de escrita.
            await BloquearEscritaAsync(numeroContaOrigem);

            var origemEntity = await _context.Contas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Numero == numeroContaOrigem);

            var destinoEntity = await _context.Contas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Numero == numeroContaDestino);

            if (origemEntity is null || destinoEntity is null)
            {
                await transaction.RollbackAsync();

                return new(
                    false,
                    "Conta de origem ou destino não encontrada.",
                    null
                );
            }

            var origem = ContaBancaria.Reconstituir(
                origemEntity.Numero,
                CriarCliente(origemEntity.Cliente),
                origemEntity.SaldoCentavos / 100m
            );

            var destino = ContaBancaria.Reconstituir(
                destinoEntity.Numero,
                CriarCliente(destinoEntity.Cliente),
                destinoEntity.SaldoCentavos / 100m
            );

            if (!origem.Transferir(destino, valor))
            {
                await transaction.RollbackAsync();

                return new(
                    false,
                    "Transferência não autorizada. Verifique o saldo disponível e o valor.",
                    null
                );
            }

            if (!TentarConverterParaCentavos(
                    origem.Saldo,
                    out long saldoOrigem) ||
                !TentarConverterParaCentavos(
                    destino.Saldo,
                    out long saldoDestino))
            {
                await transaction.RollbackAsync();

                return new(
                    false,
                    "Saldo resultante fora do limite permitido.",
                    null
                );
            }

            origemEntity.SaldoCentavos = saldoOrigem;
            destinoEntity.SaldoCentavos = saldoDestino;

            _context.Transacoes.Add(new TransacaoEntity
            {
                ContaOrigem = numeroContaOrigem,
                ContaDestino = numeroContaDestino,
                Tipo = "TRANSFERENCIA",
                Destinatario = destinoEntity.Cliente.Nome,
                ValorCentavos = valorCentavos,
                DataHora = DateTime.UtcNow,
                Status = "CONCLUIDA"
            });

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new(
                true,
                $"Transferência de R$ {valor:F2} realizada com sucesso para {destinoEntity.Cliente.Nome}.",
                origem.Saldo
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // BLOQUEIO DE ESCRITA SQLITE
    // =========================================================

    private async Task BloquearEscritaAsync(int numeroConta)
    {
        await _context.Contas
            .Where(c => c.Numero == numeroConta)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    c => c.SaldoCentavos,
                    c => c.SaldoCentavos
                )
            );

        // ExecuteUpdate não atualiza o ChangeTracker.
        // Limpamos o rastreamento para evitar entidades obsoletas.
        _context.ChangeTracker.Clear();
    }

    // =========================================================
    // RECONSTITUIÇÃO DO CLIENTE
    // =========================================================

    private static Cliente CriarCliente(ClienteEntity clienteEntity)
    {
        return new Cliente(
            clienteEntity.Id,
            clienteEntity.Nome,
            clienteEntity.DataNascimento,
            clienteEntity.Cpf,
            new Endereco(
                clienteEntity.Logradouro,
                clienteEntity.Numero,
                clienteEntity.Complemento,
                clienteEntity.Bairro,
                clienteEntity.Cidade,
                clienteEntity.Estado,
                clienteEntity.Cep,
                clienteEntity.Pais,
                clienteEntity.Zona
            ),
            clienteEntity.Telefone
        );
    }

    // =========================================================
    // VALIDAÇÃO MONETÁRIA
    // =========================================================

    private static bool TentarConverterParaCentavos(
        decimal valor,
        out long centavos)
    {
        centavos = 0;

        if (valor <= 0 || decimal.Round(valor, 2) != valor)
        {
            return false;
        }

        try
        {
            centavos = decimal.ToInt64(valor * 100m);
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }
}