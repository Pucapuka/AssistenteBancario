namespace AssistenteBancario.API.Persistence.Entities;

public class TransacaoEntity
{
    public int Id { get; set; }

    public int ContaOrigem { get; set; }

    public int? ContaDestino { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string? Destinatario { get; set; }

    public long ValorCentavos { get; set; }

    public DateTime DataHora { get; set; }

    public string Status { get; set; } = string.Empty;
}