namespace SimulacaoBanco.Models;

/// <summary>
/// Representa uma conta bancária e controla suas operações financeiras.
/// </summary>
public class ContaBancaria : IEquatable<ContaBancaria>
{
    public int Numero { get; private set; }

    public Cliente Cliente { get; }

    public decimal Saldo { get; private set; }

    /// <summary>
    /// Cria uma nova conta bancária com saldo inicial zero.
    /// </summary>
    public ContaBancaria(int numero, Cliente cliente)
    {
        if (numero <= 0)
            throw new ArgumentException(
                "Número da conta inválido.",
                nameof(numero)
            );

        Cliente = cliente
            ?? throw new ArgumentNullException(nameof(cliente));

        Numero = numero;
        Saldo = 0m;
    }

    // =========================================================
    // RECONSTITUIÇÃO DA ENTIDADE
    // =========================================================

    /// <summary>
    /// Reconstrói uma conta bancária a partir de dados persistidos.
    /// </summary>
    public static ContaBancaria Reconstituir(
        int numero,
        Cliente cliente,
        decimal saldo)
    {
        if (saldo < 0)
            throw new ArgumentOutOfRangeException(
                nameof(saldo),
                "Saldo não pode ser negativo."
            );

        if (!PossuiPrecisaoMonetariaValida(saldo))
            throw new ArgumentException(
                "Saldo deve possuir no máximo duas casas decimais.",
                nameof(saldo)
            );

        var conta = new ContaBancaria(numero, cliente);

        conta.Saldo = saldo;

        return conta;
    }

    // =========================================================
    // OPERAÇÕES FINANCEIRAS
    // =========================================================

    /// <summary>
    /// Adiciona um valor ao saldo da conta.
    /// </summary>
    /// <returns>
    /// True se o depósito foi realizado; False caso contrário.
    /// </returns>
    public bool Depositar(decimal valor)
    {
        if (!ValorMonetarioValido(valor))
            return false;

        try
        {
            decimal novoSaldo = checked(Saldo + valor);

            Saldo = novoSaldo;

            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    /// <summary>
    /// Retira um valor do saldo da conta.
    /// </summary>
    /// <returns>
    /// True se o saque foi realizado; False caso contrário.
    /// </returns>
    public bool Sacar(decimal valor)
    {
        if (!ValorMonetarioValido(valor))
            return false;

        if (valor > Saldo)
            return false;

        Saldo -= valor;

        return true;
    }

    /// <summary>
    /// Transfere um valor para outra conta bancária.
    /// </summary>
    /// <returns>
    /// True se a transferência foi realizada; False caso contrário.
    /// </returns>
    public bool Transferir(
        ContaBancaria beneficiario,
        decimal valor)
    {
        if (beneficiario is null)
            return false;

        if (beneficiario == this)
            return false;

        if (!ValorMonetarioValido(valor))
            return false;

        if (valor > Saldo)
            return false;

        try
        {
            // Calcula o novo saldo do beneficiário antes
            // de modificar qualquer uma das contas.
            decimal novoSaldoBeneficiario =
                checked(beneficiario.Saldo + valor);

            decimal novoSaldoOrigem = Saldo - valor;

            // Somente depois de todas as validações,
            // os saldos são efetivamente modificados.
            Saldo = novoSaldoOrigem;

            beneficiario.Saldo = novoSaldoBeneficiario;

            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    // =========================================================
    // CONSULTA
    // =========================================================

    /// <summary>
    /// Retorna o saldo atual da conta.
    /// </summary>
    public decimal ConsultarSaldo()
    {
        return Saldo;
    }

    // =========================================================
    // VALIDAÇÕES
    // =========================================================

    /// <summary>
    /// Verifica se um valor monetário é positivo e possui
    /// no máximo duas casas decimais.
    /// </summary>
    private static bool ValorMonetarioValido(decimal valor)
    {
        return valor > 0 &&
               PossuiPrecisaoMonetariaValida(valor);
    }

    /// <summary>
    /// Verifica se um valor possui no máximo duas casas decimais.
    /// Também aceita zero para utilização na reconstituição.
    /// </summary>
    private static bool PossuiPrecisaoMonetariaValida(
        decimal valor)
    {
        return decimal.Round(valor, 2) == valor;
    }

    // =========================================================
    // IGUALDADE ENTRE CONTAS
    // =========================================================

    public bool Equals(ContaBancaria? other)
    {
        if (other is null)
            return false;

        return Numero == other.Numero;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ContaBancaria);
    }

    public override int GetHashCode()
    {
        return Numero.GetHashCode();
    }

    public static bool operator ==(
        ContaBancaria? left,
        ContaBancaria? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (ReferenceEquals(left, null) ||
            ReferenceEquals(right, null))
            return false;

        return left.Numero == right.Numero;
    }

    public static bool operator !=(
        ContaBancaria? left,
        ContaBancaria? right)
    {
        return !(left == right);
    }
}