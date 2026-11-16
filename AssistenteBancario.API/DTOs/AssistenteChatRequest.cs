namespace AssistenteBancario.API.DTOs;

public record AssistenteChatRequest(
    int NumeroConta,
    string Mensagem
);