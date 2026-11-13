# Assistente Bancário

Assistente bancário com interface conversacional, desenvolvido com **C# / ASP.NET Core**, **Entity Framework Core**, **SQLite** e **React + TypeScript** (frontend planejado).

O objetivo é permitir que um usuário consulte informações de uma conta e solicite operações bancárias por meio de uma interface de chat. A integração com um modelo de IA usando chamadas de ferramentas (*Function Calling*) está prevista para uma próxima etapa. As operações financeiras devem ser sempre executadas e validadas pelo backend — nunca calculadas ou simuladas pelo modelo de IA.

> **Status:** backend em desenvolvimento, com operações bancárias expostas por API. A interface React e a integração com a OpenAI ainda serão implementadas. Este projeto é um protótipo educacional e não deve ser utilizado para movimentar dinheiro real.

## Funcionalidades do backend

- Consulta de saldo.
- Depósito.
- Saque.
- Transferência entre contas.
- Persistência de clientes, contas e transações com SQLite e Entity Framework Core.
- Regras e modelos de domínio separados da camada de API.
- Testes automatizados com xUnit.

## Tecnologias

### Backend
- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- xUnit

### Frontend — planejado
- React
- TypeScript
- Vite

### Inteligência artificial — planejada
- OpenAI API
- Function Calling / Tool Calling

## Estrutura do projeto

```text
AssistenteBancario/
├── AssistenteBancario.API/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Migrations/
│   ├── Persistence/
│   │   └── Entities/
│   ├── Repositories/
│   ├── Services/
│   ├── Tools/
│   └── Program.cs
├── AssistenteBancario.Domain/
│   └── Models/
├── AssistenteBancario.Tests/
│   └── Services/
└── AssistenteBancario.slnx
```

O frontend `AssistenteBancario.Web` será adicionado posteriormente.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- Node.js e npm (quando o frontend for criado)

Verifique as versões instaladas:

```bash
dotnet --version
git --version
node --version
npm --version
```

## Executar a API

Entre na pasta da solução:

```bash
cd AssistenteBancario
```

Restaure as dependências e compile:

```bash
dotnet restore AssistenteBancario.slnx
dotnet build AssistenteBancario.slnx
```

Execute a API:

```bash
dotnet run --project AssistenteBancario.API
```

A URL local depende da configuração de execução em `AssistenteBancario.API/Properties/launchSettings.json`. No ambiente de desenvolvimento utilizado durante a implementação, a API foi executada em:

```text
http://localhost:5142
```

## Endpoints disponíveis

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/contas/{numeroConta}/saldo` | Consulta o saldo |
| POST | `/api/contas/{numeroConta}/depositos` | Realiza um depósito |
| POST | `/api/contas/{numeroConta}/saques` | Realiza um saque |
| POST | `/api/contas/{numeroContaOrigem}/transferencias` | Realiza uma transferência |

### Exemplos de requisições

Consulta de saldo:

```bash
curl http://localhost:5142/api/contas/1001/saldo
```

Depósito:

```bash
curl -X POST http://localhost:5142/api/contas/1001/depositos \
  -H "Content-Type: application/json" \
  -d '{"valor": 100.00}'
```

Saque:

```bash
curl -X POST http://localhost:5142/api/contas/1001/saques \
  -H "Content-Type: application/json" \
  -d '{"valor": 50.00}'
```

Transferência:

```bash
curl -X POST http://localhost:5142/api/contas/1001/transferencias \
  -H "Content-Type: application/json" \
  -d '{"numeroContaDestino": 1002, "valor": 25.00}'
```

Os exemplos pressupõem que a API esteja em execução na porta `5142` e que as contas de demonstração tenham sido inicializadas.

## Banco de dados

O projeto utiliza SQLite com Entity Framework Core. O banco local é criado e atualizado pelas migrations da aplicação.

Os arquivos locais do banco (`*.db`, `*.sqlite` e arquivos auxiliares) são ignorados pelo Git. Portanto, eles não são enviados ao repositório. Em outro ambiente, a aplicação deverá criar/atualizar o banco conforme sua configuração e as migrations disponíveis.

## Executar os testes

Na raiz da solução:

```bash
dotnet test AssistenteBancario.slnx
```

## Configuração e segredos

Não versione chaves de API, senhas, tokens ou credenciais.

- Mantenha configurações locais e segredos fora do Git.
- Use variáveis de ambiente ou User Secrets para valores sensíveis.
- Se necessário, disponibilize arquivos de exemplo sem credenciais reais, como `.env.example` ou `appsettings.Development.example.json`.

## Próximas etapas

- [ ] Concluir a organização e validação da solução .NET.
- [ ] Ampliar a cobertura dos testes automatizados.
- [ ] Criar o frontend com React, TypeScript e Vite.
- [ ] Integrar o frontend à API.
- [ ] Implementar a camada de agente com OpenAI Function Calling.
- [ ] Exigir confirmação explícita antes de executar operações financeiras solicitadas pelo assistente.
- [ ] Implementar autenticação e autorização antes de qualquer uso além do ambiente de demonstração.

## Segurança

Este projeto é um protótipo para estudo. Antes de qualquer uso real, será necessário implementar, entre outros pontos:

- Autenticação e autorização por usuário e conta.
- Proteção de credenciais e dados pessoais.
- Validação e rastreabilidade das operações.
- Confirmação explícita de operações financeiras.
- Controles de concorrência, idempotência e tratamento de falhas.
- Revisão de segurança e testes adequados ao contexto financeiro.

A IA não deve ter acesso direto ao banco de dados nem autoridade para alterar saldos. Ela deve solicitar operações por ferramentas controladas, enquanto o backend valida permissões, regras de negócio e resultado da operação.

## Licença

Ainda não definida.
