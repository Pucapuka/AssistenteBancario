Vou substituir o README padrão do Vite por uma documentação do **Moneta AI**, apresentando o propósito do frontend, tecnologias, funcionalidades, estrutura de pastas e instruções de execução.

Mantive a distinção entre as operações manuais e as funcionalidades restritas do assistente de IA.

# Moneta AI — Frontend

Interface web de um sistema bancário desenvolvido com React e TypeScript, integrado a uma API REST em ASP.NET Core e a um assistente bancário baseado em Inteligência Artificial.

O Moneta AI permite realizar operações bancárias convencionais por meio de uma interface gráfica e interagir com um assistente inteligente utilizando linguagem natural.

## Sobre o projeto

O Moneta AI foi desenvolvido como parte de um projeto acadêmico que explora a integração de modelos de linguagem (LLMs) com sistemas bancários.

A aplicação possui dois canais de interação:

- **Operações manuais:** interface convencional para consulta de saldo, depósitos, saques e transferências.
- **Assistente bancário com IA:** interação em linguagem natural para consultar saldo, solicitar depósitos e realizar transferências entre contas.

O assistente utiliza Function Calling para identificar solicitações e acionar ferramentas específicas disponibilizadas pelo backend.

Operações financeiras iniciadas pelo assistente exigem confirmação explícita do usuário antes da execução.

## Tecnologias utilizadas

| Tecnologia | Aplicação |
|---|---|
| React 19 | Construção da interface |
| TypeScript | Tipagem estática |
| Vite | Ambiente de desenvolvimento e build |
| Axios | Comunicação com a API REST |
| Lucide React | Ícones da interface |
| CSS3 | Estilização e layout |
| Oxlint | Análise estática e linting |

## Funcionalidades

### Dashboard

- Visualização do saldo disponível.
- Ocultação e exibição do saldo.
- Seleção da conta bancária ativa.
- Cards de resumo financeiro.
- Visualização de movimentações recentes demonstrativas.
- Acesso às operações bancárias.
- Acesso ao assistente Moneta.

### Operações bancárias manuais

A interface convencional permite:

- Consultar saldo.
- Realizar depósitos.
- Realizar saques.
- Transferir valores entre contas.

As operações são enviadas diretamente à API bancária, que realiza as validações necessárias.

### Assistente bancário com IA

O assistente Moneta permite interagir com o sistema utilizando linguagem natural.

Suas funcionalidades são restritas a três operações:

1. **Consultar saldo:** consulta o saldo atualizado da conta ativa.
2. **Depositar:** solicita um depósito e aguarda a confirmação do usuário.
3. **Transferir:** solicita uma transferência entre contas e aguarda a confirmação do usuário.

#### Fluxo de confirmação

Para depósitos e transferências, o fluxo é:

1. O usuário descreve a operação em linguagem natural.
2. O assistente interpreta a solicitação.
3. O backend valida os dados e registra uma operação pendente.
4. O frontend apresenta os detalhes da operação.
5. O usuário confirma explicitamente a operação.
6. O backend executa a transação.
7. O frontend atualiza o saldo da conta.

O assistente não possui ferramentas para saques, pagamentos, empréstimos ou outras operações fora do escopo definido.

## Arquitetura do frontend

O projeto utiliza uma organização baseada na separação de responsabilidades:

- **Components:** componentes responsáveis pela interface.
- **Services:** comunicação com a API REST.
- **Types:** definição das estruturas e contratos de dados utilizados pela aplicação.
- **Assets:** recursos estáticos.

### Estrutura de diretórios

```text
AssistenteBancario.Web/
│
├── public/
│
├── src/
│   ├── assets/
│   │
│   ├── components/
│   │   ├── Dashboard/
│   │   │   └── Dashboard.tsx
│   │   │
│   │   ├── Header/
│   │   │   └── Header.tsx
│   │   │
│   │   ├── Operations/
│   │   │   └── Operations.tsx
│   │   │
│   │   └── Sidebar/
│   │       └── Sidebar.tsx
│   │
│   ├── services/
│   │   ├── api.ts
│   │   ├── bancoService.ts
│   │   └── assistenteService.ts
│   │
│   ├── types/
│   │   └── banco.ts
│   │
│   ├── App.tsx
│   ├── App.css
│   ├── index.css
│   └── main.tsx
│
├── index.html
├── package.json
├── tsconfig.json
└── vite.config.ts
```

## Integração com o backend

O frontend consome a API desenvolvida em ASP.NET Core.

A comunicação HTTP é centralizada utilizando uma instância do Axios.

Arquivo: `src/services/api.ts`

```typescript
import axios from "axios";

export const api = axios.create({
  baseURL: "http://localhost:5142/api",
  headers: {
    "Content-Type": "application/json",
  },
});
```

### Endpoints utilizados

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/contas/{numeroConta}/saldo` | Consulta saldo |
| POST | `/api/contas/{numeroConta}/depositos` | Depósito manual |
| POST | `/api/contas/{numeroConta}/saques` | Saque manual |
| POST | `/api/contas/{numeroContaOrigem}/transferencias` | Transferência manual |
| POST | `/api/assistente/chat` | Conversação com o agente |
| POST | `/api/assistente/confirmar/{id}` | Confirmação de operação pendente |

### Serviços

**bancoService.ts**

Responsável pelas operações bancárias convencionais:

- `consultarSaldo()`
- `depositar()`
- `sacar()`
- `transferir()`

**assistenteService.ts**

Responsável pela comunicação com o agente bancário:

- `conversarComAssistente()`
- `confirmarOperacao()`

## Comunicação com o assistente

O frontend envia as mensagens para o backend por meio de uma requisição HTTP.

Exemplo de solicitação:

```json
{
  "numeroConta": 1001,
  "mensagem": "Quero transferir R$ 100,00 para a conta 1002."
}
```

Exemplo de resposta com operação pendente:

```json
{
  "mensagem": "Sua solicitação de transferência foi registrada.",
  "operacaoPendente": {
    "id": "identificador-da-operacao",
    "tipo": "Transferencia",
    "numeroContaOrigem": 1001,
    "numeroContaDestino": 1002,
    "valor": 100,
    "expiraEm": "2026-11-16T19:22:40+00:00"
  }
}
```

O identificador recebido é utilizado para confirmar a operação por meio do endpoint específico.

A validação definitiva e a execução das transações são responsabilidades do backend.

## Pré-requisitos

Antes de executar o frontend, é necessário ter instalado:

- Node.js
- npm
- Git

Também é necessário que o backend ASP.NET Core esteja configurado e em execução.

## Instalação e execução

### 1. Clonar o repositório

```bash
git clone https://github.com/Pucapuka/AssistenteBancario.git
```

### 2. Acessar o frontend

```bash
cd AssistenteBancario/AssistenteBancario.Web
```

### 3. Instalar as dependências

```bash
npm install
```

### 4. Iniciar o ambiente de desenvolvimento

```bash
npm run dev
```

Por padrão, o Vite disponibiliza a aplicação em:

```text
http://localhost:5173
```

### 5. Gerar build de produção

```bash
npm run build
```

### 6. Visualizar o build

```bash
npm run preview
```

## Configuração do backend

Por padrão, o Axios está configurado para consumir:

```text
http://localhost:5142/api
```

Caso o backend esteja executando em outra porta, atualize a propriedade `baseURL` em:

```text
src/services/api.ts
```

O backend também precisa permitir requisições de origem `http://localhost:5173` por meio da configuração de CORS.

## Observações

- Os saldos são obtidos diretamente da API bancária.
- As operações manuais e as operações realizadas pelo assistente utilizam o mesmo backend.
- O assistente possui escopo restrito às três funcionalidades definidas para o projeto.
- Depósitos e transferências solicitados pelo assistente dependem de confirmação explícita.
- Os cards de entradas, saídas e movimentações recentes ainda utilizam dados demonstrativos.

## Projeto acadêmico

**Moneta AI — Assistente Bancário com Inteligência Artificial**

Projeto desenvolvido para demonstrar a integração entre aplicações web, APIs REST, persistência de dados e modelos de linguagem utilizando Function Calling.

---
