import { api } from "./api";
import type {
  AssistenteChatRequest,
  AssistenteChatResponse,
  OperacaoBancariaResponse,
} from "../types/banco";

export async function conversarComAssistente(
  dados: AssistenteChatRequest
): Promise<AssistenteChatResponse> {
  const response = await api.post<AssistenteChatResponse>(
    "/assistente/chat",
    dados
  );

  return response.data;
}

export async function confirmarOperacao(
  id: string
): Promise<OperacaoBancariaResponse> {
  const response = await api.post<OperacaoBancariaResponse>(
    `/assistente/confirmar/${id}`
  );

  return response.data;
}