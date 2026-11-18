export interface OperacaoBancariaResponse {
  sucesso: boolean;
  mensagem: string;
  saldoAtual: number | null;
}

export interface OperacaoPendente {
  id: string;
  tipo: "Deposito" | "Transferencia";
  numeroContaOrigem: number;
  numeroContaDestino: number | null;
  valor: number;
  expiraEm: string;
}

export interface AssistenteChatRequest {
  numeroConta: number;
  mensagem: string;
}

export interface AssistenteChatResponse {
  mensagem: string;
  operacaoPendente: OperacaoPendente | null;
}

export interface TransacaoHistorico {
  id: number;
  tipo: string;
  direcao: "ENTRADA" | "SAIDA" | "INDEFINIDA";
  numeroContaOrigem: number;
  numeroContaDestino: number | null;
  destinatario: string | null;
  valor: number;
  dataHora: string;
  status: string;
}

export interface HistoricoTransacoesResponse {
  sucesso: boolean;
  mensagem: string;
  transacoes: TransacaoHistorico[];
}