namespace AssistenteBancario.API.Persistence.Entities;

public class ClienteEntity
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public DateOnly DataNascimento { get; set; }

    public string Cpf { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public int Numero { get; set; }

    public string Complemento { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Cep { get; set; } = string.Empty;

    public string Pais { get; set; } = string.Empty;

    public string Zona { get; set; } = string.Empty;

    public ContaEntity? Conta { get; set; }
}