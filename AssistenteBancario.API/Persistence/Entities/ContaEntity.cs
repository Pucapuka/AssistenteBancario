namespace AssistenteBancario.API.Persistence.Entities;

public class ContaEntity
{
    public int Numero { get; set; }

    public int ClienteId { get; set; }

    public long SaldoCentavos { get; set; }

    public ClienteEntity Cliente { get; set; } = null!;
}