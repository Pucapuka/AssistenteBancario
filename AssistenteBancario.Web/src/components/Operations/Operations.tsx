import { useState, type FormEvent } from "react";
import {
    X,
    ArrowDownLeft,
    ArrowUpRight,
    ArrowLeftRight,
  } from "lucide-react";
  
  import {
    depositar,
    sacar,
    transferir,
  } from "../../services/bancoService";

type TipoOperacao = "deposito" | "saque" | "transferencia";

interface OperationsProps {
  onClose: () => void;
  onSuccess: (novoSaldo: number) => void;
  numeroConta: number;
}

function Operations({ onClose, onSuccess }: OperationsProps) {
  const [tipo, setTipo] = useState<TipoOperacao>("deposito");
  const [valor, setValor] = useState("");
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");
  const [numeroContaDestino, setNumeroContaDestino] = useState(""); 

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setErro("");
    setSucesso("");

    const valorNumerico = Number(valor);

    if (!Number.isFinite(valorNumerico) || valorNumerico <= 0) {
      setErro("Informe um valor maior que zero.");
      return;
    }

    if (!Number.isInteger(valorNumerico * 100)) {
      setErro("Informe um valor com no máximo duas casas decimais.");
      return;
    }

    if (tipo === "transferencia") {
        const destino = Number(numeroContaDestino);
      
        if (!Number.isInteger(destino) || destino <= 0) {
          setErro("Informe um número de conta válido.");
          return;
        }
      
        if (destino === 1001) {
          setErro("A conta de destino deve ser diferente da conta de origem.");
          return;
        }
    }

    const descricao =
        tipo === "deposito"
            ? "depósito"
            : tipo === "saque"
                ? "saque"
                : "transferência";

    const mensagemConfirmacao =
      tipo === "transferencia"
        ? `Confirma a transferência de ${formatCurrency(valorNumerico)} da conta 1001 para a conta ${numeroContaDestino}?`
        : `Confirma ${descricao} de ${formatCurrency(valorNumerico)} na conta 1001?`;

    const confirmado = window.confirm(mensagemConfirmacao);

    if (!confirmado) return;
    
    try {
      setCarregando(true);

      const resultado =
        tipo === "deposito"
        ? await depositar(1001, valorNumerico)
        : tipo === "saque"
            ? await sacar(1001, valorNumerico)
            : await transferir(
                1001,
                Number(numeroContaDestino),
                valorNumerico
            );

      if (!resultado.sucesso) {
        throw new Error(resultado.mensagem);
      }

      if (resultado.saldoAtual === null) {
        throw new Error("A operação foi processada, mas o saldo não foi retornado.");
      }

      onSuccess(resultado.saldoAtual);
      setSucesso(resultado.mensagem);
      setValor("");
      setNumeroContaDestino("");
    } catch (error: unknown) {
      console.error("Erro na operação bancária:", error);

      if (typeof error === "object" && error !== null && "response" in error) {
        const apiError = error as {
          response?: {
            data?: {
              mensagem?: string;
            };
          };
        };

        setErro(
          apiError.response?.data?.mensagem ??
            "Não foi possível realizar a operação."
        );
      } else if (error instanceof Error) {
        setErro(error.message);
      } else {
        setErro("Ocorreu um erro inesperado.");
      }
    } finally {
      setCarregando(false);
    }
  }

  function formatCurrency(value: number) {
    return value.toLocaleString("pt-BR", {
      style: "currency",
      currency: "BRL",
    });
  }

  return (
    <div className="operation-overlay">
      <section
        className="operation-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="operation-title"
      >
        <div className="operation-header">
          <div>
            <span>CONTA Nº 1001</span>
            <h2 id="operation-title">Nova operação</h2>
          </div>

          <button
            type="button"
            className="operation-close"
            onClick={onClose}
            aria-label="Fechar"
          >
            <X size={21} />
          </button>
        </div>

        <div className="operation-tabs">
            <button
              type="button"
              className={tipo === "deposito" ? "selected" : ""}
              onClick={() => {
                setTipo("deposito");
                setErro("");
                setSucesso("");
              }}
            >
              <ArrowDownLeft size={18} />
              Depositar
            </button>
          
            <button
              type="button"
              className={tipo === "saque" ? "selected" : ""}
              onClick={() => {
                setTipo("saque");
                setErro("");
                setSucesso("");
              }}
            >
              <ArrowUpRight size={18} />
              Sacar
            </button>
          
            <button
              type="button"
              className={tipo === "transferencia" ? "selected" : ""}
              onClick={() => {
                setTipo("transferencia");
                setErro("");
                setSucesso("");
              }}
            >
              <ArrowLeftRight size={18} />
              Transferir
            </button>
        </div>

        <form onSubmit={handleSubmit}>
          
            {tipo === "transferencia" && (
            <>
              <label htmlFor="account-destination">
                Número da conta de destino
              </label>
                  
              <input
                id="account-destination"
                type="number"
                min="1"
                step="1"
                placeholder="Ex.: 1002"
                value={numeroContaDestino}
                onChange={(event) => setNumeroContaDestino(event.target.value)}
                required
              />
            </>
          )}

          <label htmlFor="operation-value">Valor da operação</label>

          <div className="operation-input">
            <span>R$</span>
            <input
              id="operation-value"
              type="number"
              min="0.01"
              step="0.01"
              placeholder="0,00"
              value={valor}
              onChange={(event) => setValor(event.target.value)}
              required
            />
          </div>

          {erro && (
            <p className="operation-feedback error" role="alert">
              {erro}
            </p>
          )}

          {sucesso && (
            <p className="operation-feedback success" role="status">
              {sucesso}
            </p>
          )}

          <div className="operation-actions">
            <button
              type="button"
              className="operation-cancel"
              onClick={onClose}
              disabled={carregando}
            >
              Cancelar
            </button>

            <button
              type="submit"
              className="operation-confirm"
              disabled={carregando}
            >
              {carregando
                ? "Processando..."
                : tipo === "deposito"
                  ? "Continuar depósito"
                  : tipo === "saque"
                    ? "Continuar saque"
                    : "Continuar transferência"
              }
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}

export default Operations;