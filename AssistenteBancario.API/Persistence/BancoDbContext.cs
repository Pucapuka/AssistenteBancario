using AssistenteBancario.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssistenteBancario.API.Persistence;

public class BancoDbContext : DbContext
{
    public BancoDbContext(DbContextOptions<BancoDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();

    public DbSet<ContaEntity> Contas => Set<ContaEntity>();

    public DbSet<TransacaoEntity> Transacoes => Set<TransacaoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CLIENTES
        modelBuilder.Entity<ClienteEntity>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.Cpf)
                .IsUnique();

            entity.Property(c => c.Nome)
                .IsRequired();

            entity.Property(c => c.Cpf)
                .IsRequired();
        });

        // CONTAS
        modelBuilder.Entity<ContaEntity>(entity =>
        {
            entity.HasKey(c => c.Numero);

            entity.HasOne(c => c.Cliente)
                .WithOne(c => c.Conta)
                .HasForeignKey<ContaEntity>(c => c.ClienteId);

            entity.Property(c => c.SaldoCentavos)
                .IsRequired();
        });

        // TRANSAÇÕES
        modelBuilder.Entity<TransacaoEntity>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Tipo)
                .IsRequired();

            entity.Property(t => t.ValorCentavos)
                .IsRequired();

            entity.Property(t => t.Status)
                .IsRequired();

            entity.HasIndex(t => t.ContaOrigem);

            entity.HasIndex(t => t.ContaDestino);

            // Conta de origem
            entity.HasOne<ContaEntity>()
                .WithMany()
                .HasForeignKey(t => t.ContaOrigem)
                .OnDelete(DeleteBehavior.Restrict);

            // Conta de destino
            entity.HasOne<ContaEntity>()
                .WithMany()
                .HasForeignKey(t => t.ContaDestino)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}