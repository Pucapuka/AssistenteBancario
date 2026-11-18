import { useState, useEffect, type FormEvent } from "react";

import {
  Plus,
  ArrowDownLeft,
  ArrowUpRight,
  Send,
  Sparkles,
  CreditCard,
  Eye,
} from "lucide-react";

import {
  conversarComAssistente,
  confirmarOperacao,
} from "../../services/assistenteService";

import type { OperacaoPendente, TransacaoHistorico } from "../../types/banco";
import { consultarTransacoes } from "../../services/bancoService";

type Message = {
  id: string;
  text: string;
  sender: "user" | "assistant";
  operacaoPendente?: OperacaoPendente;
};

const WELCOME_DATE = new Intl.DateTimeFormat("pt-BR", {
  weekday: "long",
  day: "2-digit",
  month: "long",
})
  .format(new Date())
  .toUpperCase();

interface DashboardProps {
  saldo: number | null;
  numeroConta: number;
  onSelectAccount: (numero: number) => void;
  carregandoSaldo: boolean;
  erroSaldo: string | null;
  onOpenOperations: () => void;
  onRefreshSaldo: () => Promise<void>;
}

function formatCurrency(value: number) {
  return value.toLocaleString("pt-BR", {
    style: "currency",
    currency: "BRL",
  });
}

function formatarData(data: string) {
  return new Date(data).toLocaleString("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function obterMensagemErro(error: unknown, mensagemPadrao: string): string {
  if (typeof error === "object" && error !== null && "response" in error) {
    const apiError = error as {
      response?: {
        data?: {
          mensagem?: string;
        };
      };
    };

    const mensagemApi = apiError.response?.data?.mensagem;

    if (mensagemApi) {
      return mensagemApi;
    }
  }

  if (error instanceof Error) {
    return error.message;
  }

  return mensagemPadrao;
}

function Dashboard({
  saldo,
  carregandoSaldo,
  erroSaldo,
  numeroConta,
  onSelectAccount,
  onOpenOperations,
  onRefreshSaldo,
}: DashboardProps) {
  const [showBalance, setShowBalance] = useState(true);
  const [message, setMessage] = useState("");

  const [enviando, setEnviando] = useState(false);
  const [confirmandoId, setConfirmandoId] = useState<string | null>(null);

  const [messages, setMessages] = useState<Message[]>([
    {
      id: "welcome",
      sender: "assistant",
      text: "Olá! Sou seu assistente bancário. Como posso ajudar você hoje?",
    },
  ]);

  const [transacoes, setTransacoes] = useState<TransacaoHistorico[]>([]);
  const totalEntradas = transacoes
  .filter((transacao) => transacao.direcao === "ENTRADA")
  .reduce((total, transacao) => total + transacao.valor, 0);

  const totalSaidas = transacoes
  .filter((transacao) => transacao.direcao === "SAIDA")
  .reduce((total, transacao) => total + transacao.valor, 0);
  const [carregandoTransacoes, setCarregandoTransacoes] = useState(false);
  const [erroTransacoes, setErroTransacoes] = useState<string | null>(null);

  //Carrega histórico de transações
  async function carregarTransacoes() {
    setCarregandoTransacoes(true);
    setErroTransacoes(null);
  
    try {
      const resultado = await consultarTransacoes(numeroConta);
      setTransacoes(resultado.transacoes);
    } catch (error) {
      console.error("Erro ao carregar transações:", error);
      setErroTransacoes("Não foi possível carregar o histórico.");
    } finally {
      setCarregandoTransacoes(false);
    }
  }

  useEffect(() => {
    carregarTransacoes();
  }, [numeroConta]);

  // Envia a mensagem para o agente bancário.
  async function handleSendMessage(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const texto = message.trim();

    if (!texto || enviando) return;

    setMessages((current) => [
      ...current,
      {
        id: crypto.randomUUID(),
        sender: "user",
        text: texto,
      },
    ]);

    setMessage("");
    setEnviando(true);

    try {
      const resposta = await conversarComAssistente({
        numeroConta,
        mensagem: texto,
      });

      setMessages((current) => [
        ...current,
        {
          id: crypto.randomUUID(),
          sender: "assistant",
          text: resposta.mensagem,
          operacaoPendente: resposta.operacaoPendente ?? undefined,
        },
      ]);
    } catch (error: unknown) {
      console.error("Erro ao conversar com o assistente:", error);

      const mensagemErro = obterMensagemErro(
        error,
        "Não foi possível comunicar com o assistente. Tente novamente."
      );

      setMessages((current) => [
        ...current,
        {
          id: crypto.randomUUID(),
          sender: "assistant",
          text: mensagemErro,
        },
      ]);
    } finally {
      setEnviando(false);
    }
  }

  // Confirma uma operação que foi previamente validada pelo backend.
  async function handleConfirmarOperacao(operacao: OperacaoPendente) {
    if (confirmandoId !== null) return;

    setConfirmandoId(operacao.id);

    try {
      const resultado = await confirmarOperacao(operacao.id);

      if (!resultado.sucesso) {
        throw new Error(resultado.mensagem);
      }

      // Remove o cartão pendente e apresenta o resultado da operação.
      setMessages((current) => [
        ...current.map((item) =>
          item.operacaoPendente?.id === operacao.id
            ? { ...item, operacaoPendente: undefined }
            : item
        ),
        {
          id: crypto.randomUUID(),
          sender: "assistant",
          text: resultado.mensagem,
        },
      ]);

      // Consulta novamente o saldo da conta ativa.
      await onRefreshSaldo();
    } catch (error: unknown) {
      console.error("Erro ao confirmar operação:", error);

      const mensagemErro = obterMensagemErro(
        error,
        "Não foi possível confirmar a operação."
      );

      setMessages((current) => [
        ...current,
        {
          id: crypto.randomUUID(),
          sender: "assistant",
          text: mensagemErro,
        },
      ]);
    } finally {
      setConfirmandoId(null);
    }
  }

  return (
    <div className="dashboard-content">
      <section className="welcome-section">
        <div>
          <span className="welcome-date">{WELCOME_DATE}</span>

          <h2>Olá, Paulo 👋</h2>
          <p>Acompanhe sua vida financeira em um só lugar.</p>
        </div>

        <button
          type="button"
          className="primary-button"
          onClick={onOpenOperations}
        >
          <Plus size={18} />
          Nova operação
        </button>
      </section>

      <section className="summary-grid">
        <article className="balance-card">
          <div className="card-heading">
            <span>Saldo disponível</span>

            <button
              type="button"
              className="balance-visibility"
              onClick={() => setShowBalance((current) => !current)}
              aria-label="Mostrar ou ocultar saldo"
            >
              <Eye size={19} />
            </button>
          </div>

          <h2>
            {!showBalance
              ? "R$ ••••••"
              : carregandoSaldo
                ? "Carregando..."
                : erroSaldo
                  ? "Indisponível"
                  : formatCurrency(saldo ?? 0)}
          </h2>

          {erroSaldo && (
            <small className="balance-error" role="alert">
              {erroSaldo}
            </small>
          )}

          <div className="account-selector">
            <label htmlFor="account-select">Conta ativa:</label>

            <select
              id="account-select"
              value={numeroConta}
              onChange={(event) =>
                onSelectAccount(Number(event.target.value))
              }
            >
              <option value={1001}>1001</option>
              <option value={1002}>1002</option>
            </select>
          </div>

          <div className="balance-footer">
            <span>Conta corrente</span>
            <CreditCard size={23} />
          </div>
        </article>

        <article className="summary-card">
          <div className="summary-icon income-icon">
            <ArrowDownLeft size={21} />
          </div>

          <span>Entradas</span>
          <h3>{formatCurrency(totalEntradas)}</h3>
          <small>Valores demonstrativos</small>
        </article>

        <article className="summary-card">
          <div className="summary-icon expense-icon">
            <ArrowUpRight size={21} />
          </div>

          <span>Saídas</span>
          <h3>{formatCurrency(totalSaidas)}</h3>
          <small>Valores demonstrativos</small>
        </article>
      </section>

      <section className="content-grid">
        <article className="transactions-card" id="transactions">
          <div className="section-heading">
            <div>
              <h3>Movimentações recentes</h3>
              <p>Acompanhe suas últimas transações.</p>
            </div>

            <button type="button" className="text-button">
              Ver todas
            </button>
          </div>

          <div className="transaction-list">
            {carregandoTransacoes ? (
              <p>Carregando movimentações...</p>
            ) : erroTransacoes ? (
              <p className="balance-error">{erroTransacoes}</p>
            ) : transacoes.length === 0 ? (
              <p>Nenhuma movimentação encontrada.</p>
            ) : (
              transacoes.slice(0, 5).map((transaction) => {
                const isEntrada = transaction.direcao === "ENTRADA";
              
                const title =
                  transaction.tipo === "DEPOSITO"
                    ? "Depósito"
                    : transaction.tipo === "SAQUE"
                      ? "Saque"
                      : isEntrada
                        ? "Transferência recebida"
                        : "Transferência enviada";
              
                const category =
                  transaction.tipo === "DEPOSITO"
                    ? "Depósito em conta"
                    : transaction.tipo === "SAQUE"
                      ? "Saque em conta"
                      : isEntrada
                        ? `Origem: ${transaction.numeroContaOrigem}`
                        : `Destino: ${transaction.numeroContaDestino}`;
              
                return (
                  <div className="transaction-item" key={transaction.id}>
                    <div className={`transaction-icon ${isEntrada ? "income" : "expense"}`}>
                      {isEntrada ? (
                        <ArrowDownLeft size={19} />
                      ) : (
                        <ArrowUpRight size={19} />
                      )}
                    </div>
                    
                    <div className="transaction-info">
                      <strong>{title}</strong>
                      <span>{category}</span>
                    </div>
                    
                    <div className="transaction-value">
                      <strong className={isEntrada ? "income" : "expense"}>
                        {isEntrada ? "+" : "-"}
                        {formatCurrency(transaction.valor)}
                      </strong>
                    
                      <span>{formatarData(transaction.dataHora)}</span>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </article>

        <article className="assistant-card" id="assistant">
          <div className="assistant-heading">
            <div className="assistant-avatar">
              <Sparkles size={21} />
            </div>

            <div>
              <h3>Assistente SelfinAI</h3>
              <span>
                <i /> Assistente bancário
              </span>
            </div>
          </div>

          <div className="chat-messages" aria-live="polite">
            {messages.map((item) => (
              <div
                key={item.id}
                className={`message ${item.sender}`}
              >
                {item.sender === "assistant" && (
                  <div className="message-avatar">
                    <Sparkles size={15} />
                  </div>
                )}

                <div className="message-content">
                  <p>{item.text}</p>

                  {item.operacaoPendente && (
                    <div className="pending-operation">
                      <div className="pending-operation-header">
                        <strong>
                          {item.operacaoPendente.tipo === "Deposito"
                            ? "Confirmação de depósito"
                            : "Confirmação de transferência"}
                        </strong>

                        <span className="pending-status">
                          Aguardando
                        </span>
                      </div>

                      <div className="pending-operation-details">
                        <div>
                          <span>Conta de origem</span>
                          <strong>
                            {item.operacaoPendente.numeroContaOrigem}
                          </strong>
                        </div>

                        {item.operacaoPendente.numeroContaDestino !==
                          null && (
                          <div>
                            <span>Conta de destino</span>
                            <strong>
                              {item.operacaoPendente.numeroContaDestino}
                            </strong>
                          </div>
                        )}

                        <div>
                          <span>Valor</span>
                          <strong>
                            {formatCurrency(item.operacaoPendente.valor)}
                          </strong>
                        </div>
                      </div>

                      <button
                        type="button"
                        className="pending-confirm-button"
                        onClick={() =>
                          handleConfirmarOperacao(item.operacaoPendente!)
                        }
                        disabled={confirmandoId !== null}
                      >
                        {confirmandoId === item.operacaoPendente.id
                          ? "Confirmando..."
                          : "Confirmar operação"}
                      </button>

                      <small>
                        A operação somente será executada após sua
                        confirmação.
                      </small>
                    </div>
                  )}
                </div>
              </div>
            ))}

            {enviando && (
              <div className="message assistant">
                <div className="message-avatar">
                  <Sparkles size={15} />
                </div>

                <p>Estou analisando sua solicitação...</p>
              </div>
            )}
          </div>

          <div className="suggestions">
            <button
              type="button"
              onClick={() => setMessage("Qual é meu saldo?")}
              disabled={enviando}
            >
              Consultar saldo
            </button>

            <button
              type="button"
              onClick={() =>
                setMessage("Quero fazer uma transferência.")
              }
              disabled={enviando}
            >
              Transferência
            </button>
          </div>

          <form className="chat-input" onSubmit={handleSendMessage}>
            <input
              type="text"
              placeholder="Digite sua mensagem..."
              value={message}
              onChange={(event) => setMessage(event.target.value)}
              disabled={enviando}
              aria-label="Mensagem para o assistente"
            />

            <button
              type="submit"
              aria-label="Enviar mensagem"
              disabled={enviando || !message.trim()}
            >
              <Send size={18} />
            </button>
          </form>

          <span className="chat-disclaimer">
            Assistente conectado ao sistema bancário.
          </span>
        </article>
      </section>
    </div>
  );
}

export default Dashboard;