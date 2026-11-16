using System.Text.Json.Nodes;

namespace AssistenteBancario.API.AI;

public static class ToolDefinitions
{
    public static JsonArray Criar()
    {
        return new JsonArray
        {
            // 1. Consultar saldo
            new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = "consultar_saldo",
                    ["description"] =
                        "Consulta o saldo atual de uma conta bancária.",

                    ["parameters"] = new JsonObject
                    {
                        ["type"] = "object",

                        ["properties"] = new JsonObject
                        {
                            ["numeroConta"] = new JsonObject
                            {
                                ["type"] = "integer",
                                ["description"] =
                                    "Número da conta bancária."
                            }
                        },

                        ["required"] = new JsonArray
                        {
                            JsonValue.Create("numeroConta")
                        },

                        ["additionalProperties"] = false
                    }
                }
            },

            // 2. Depositar
            new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = "depositar",

                    ["description"] =
                        "Registra uma solicitação de depósito pendente " +
                        "de confirmação. Não executa o depósito. " +
                        "Deve ser chamada quando o usuário solicitar " +
                        "um depósito com os parâmetros necessários.",

                    ["parameters"] = new JsonObject
                    {
                        ["type"] = "object",

                        ["properties"] = new JsonObject
                        {
                            ["numeroConta"] = new JsonObject
                            {
                                ["type"] = "integer",
                                ["description"] =
                                    "Número da conta que receberá o depósito."
                            },

                            ["valor"] = new JsonObject
                            {
                                ["type"] = "number",
                                ["description"] =
                                    "Valor positivo do depósito em reais."
                            }
                        },

                        ["required"] = new JsonArray
                        {
                            JsonValue.Create("numeroConta"),
                            JsonValue.Create("valor")
                        },

                        ["additionalProperties"] = false
                    }
                }
            },

            // 3. Transferir
            new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = "transferir",

                    ["description"] =
                        "Registra uma solicitação de transferência pendente " +
                        "de confirmação. Não executa a transferência. " +
                        "Deve ser chamada quando o usuário solicitar " +
                        "uma transferência com os parâmetros necessários.",

                    ["parameters"] = new JsonObject
                    {
                        ["type"] = "object",

                        ["properties"] = new JsonObject
                        {
                            ["numeroContaOrigem"] = new JsonObject
                            {
                                ["type"] = "integer",
                                ["description"] =
                                    "Número da conta de origem."
                            },

                            ["numeroContaDestino"] = new JsonObject
                            {
                                ["type"] = "integer",
                                ["description"] =
                                    "Número da conta de destino."
                            },

                            ["valor"] = new JsonObject
                            {
                                ["type"] = "number",
                                ["description"] =
                                    "Valor positivo da transferência em reais."
                            }
                        },

                        ["required"] = new JsonArray
                        {
                            JsonValue.Create("numeroContaOrigem"),
                            JsonValue.Create("numeroContaDestino"),
                            JsonValue.Create("valor")
                        },

                        ["additionalProperties"] = false
                    }
                }
            }
        };
    }
}