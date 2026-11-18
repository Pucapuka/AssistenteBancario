import { api } from "./api";
import type { OperacaoBancariaResponse, HistoricoTransacoesResponse } from "../types/banco";

export async function consultarSaldo(
  numeroConta: number
): Promise<OperacaoBancariaResponse> {
  const response = await api.get<OperacaoBancariaResponse>(
    `/contas/${numeroConta}/saldo`
  );

  return response.data;
}

export async function depositar(
  numeroConta: number,
  valor: number
): Promise<OperacaoBancariaResponse> {
  const response = await api.post<OperacaoBancariaResponse>(
    `/contas/${numeroConta}/depositos`,
    { valor }
  );

  return response.data;
}

export async function sacar(
  numeroConta: number,
  valor: number
): Promise<OperacaoBancariaResponse> {
  const response = await api.post<OperacaoBancariaResponse>(
    `/contas/${numeroConta}/saques`,
    { valor }
  );

  return response.data;
}

export async function transferir(
  numeroContaOrigem: number,
  numeroContaDestino: number,
  valor: number
): Promise<OperacaoBancariaResponse> {
  const response = await api.post<OperacaoBancariaResponse>(
    `/contas/${numeroContaOrigem}/transferencias`,
    {
      numeroContaDestino,
      valor,
    }
  );

  return response.data;
}

export async function consultarTransacoes(
  numeroConta: number
): Promise<HistoricoTransacoesResponse> {
  const response = await api.get<HistoricoTransacoesResponse>(
    `/contas/${numeroConta}/transacoes`
  );

  return response.data;
}