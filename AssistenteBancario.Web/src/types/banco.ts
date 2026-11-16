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