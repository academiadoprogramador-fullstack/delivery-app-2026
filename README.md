# DeliveryApp

Desenvolvido durante o curso Fullstack da [Academia do Programador 2026](https://www.academiadoprogramador.net).

API REST em .NET 10 para gerenciamento de clientes e estabelecimentos de uma plataforma de pedidos e entregas, com ASP.NET Core Identity, autenticação JWT e persistência em PostgreSQL.

## Referência funcional

### Entidade `Cliente`

| Propriedade | Descrição                                                                    |
| ----------- | ---------------------------------------------------------------------------- |
| `Id`        | Chave primária compartilhada e chave estrangeira do usuário na relação 1:1.  |
| `Nome`      | Nome do cliente, com 2 a 100 caracteres.                                     |
| `Cpf`       | Documento único do cliente, composto por exatamente 11 dígitos.              |

### Cadastro de clientes

- cria o cliente e o usuário do Identity com o mesmo identificador (`Guid` versão 7);
- exige nome com 2 a 100 caracteres;
- exige CPF com exatamente 11 dígitos;
- impede a duplicidade de CPF por validação e índice único no banco;
- exige email único;
- exige senha com pelo menos 8 caracteres, um dígito e um caractere não alfanumérico;
- associa o usuário ao papel `Cliente`;
- retorna um token JWT após o cadastro.

### Autenticação

- autentica o cliente por email e senha;
- bloqueia a conta por 5 minutos após 5 tentativas malsucedidas;
- retorna uma mensagem genérica quando as credenciais são inválidas;
- emite um JWT assinado com HMAC SHA-256 contendo o identificador, o email e o papel do usuário;
- utiliza validade configurável, de 60 minutos por padrão, e tolerância de 30 segundos na validação.

### Endpoints

| Método | Rota                        | Acesso  | Descrição                         |
| ------ | --------------------------- | ------- | --------------------------------- |
| `POST` | `/api/clientes/cadastro`    | Público | Cadastra e autentica um cliente.  |
| `POST` | `/api/clientes/login`       | Público | Autentica um cliente.             |
| `GET`  | `/api/clientes/{clienteId}` | Cliente | Consulta um cliente pelo seu ID.  |

Os demais endpoints ficam protegidos por uma política global que exige autenticação. Rotas públicas precisam ser marcadas explicitamente com `AllowAnonymous`.

Exemplo de cadastro:

```json
{
  "nome": "Cliente Exemplo",
  "cpf": "12345678901",
  "email": "cliente@example.com",
  "senha": "senha@123"
}
```

O cadastro responde com `201 Created`; o login, com `200 OK`. Ambos retornam o mesmo formato:

```json
{
  "clienteId": "01900000-0000-7000-8000-000000000000",
  "accessToken": "token-jwt",
  "dataExpiracaoEmUtc": "2026-09-01T13:00:00Z"
}
```

Erros HTTP seguem o formato Problem Details e incluem o `traceId` quando tratados pelo pipeline global.

### Entidade `Estabelecimento`

| Propriedade         | Descrição                                                                            |
| ------------------- | ------------------------------------------------------------------------------------ |
| `Id`                | Chave primária compartilhada e chave estrangeira do usuário na relação 1:1.          |
| `NomeComercial`     | Nome utilizado comercialmente.                                                       |
| `Documento`         | CPF ou CNPJ do estabelecimento.                                                       |
| `Endereco`          | Endereço do estabelecimento.                                                          |
| `Telefone`          | Telefone para contato.                                                                |
| `HorarioAbertura`   | Início do período diário de atendimento.                                              |
| `HorarioFechamento` | Final do período diário de atendimento.                                               |
| `AreaAtendimento`   | Descrição das regiões atendidas.                                                      |
| `Ativo`             | Indica se o estabelecimento está disponível para receber novos pedidos.               |

### Endpoints de estabelecimentos

| Método  | Rota                                                  | Acesso                     | Descrição                                |
| ------- | ----------------------------------------------------- | -------------------------- | ---------------------------------------- |
| `POST`  | `/api/estabelecimentos/cadastro`                      | Público                    | Cadastra e autentica um estabelecimento. |
| `POST`  | `/api/estabelecimentos/login`                         | Público                    | Autentica um estabelecimento.            |
| `GET`   | `/api/estabelecimentos`                               | Cliente ou Estabelecimento | Lista estabelecimentos disponíveis.      |
| `GET`   | `/api/estabelecimentos/disponiveis`                   | Cliente ou Estabelecimento | Lista estabelecimentos disponíveis.      |
| `GET`   | `/api/estabelecimentos/{estabelecimentoId}`           | Cliente ou Estabelecimento | Consulta um estabelecimento.             |
| `PUT`   | `/api/estabelecimentos/{estabelecimentoId}`           | Estabelecimento            | Edita o estabelecimento vinculado.       |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/ativar`    | Estabelecimento            | Ativa o estabelecimento vinculado.       |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/desativar` | Estabelecimento            | Desativa o estabelecimento vinculado.    |

## Arquitetura

A solução está dividida em quatro projetos:

| Projeto                      | Responsabilidade                                                          |
| ---------------------------- | ------------------------------------------------------------------------- |
| `DeliveryApp.Dominio`        | Entidades, contratos compartilhados e validações de domínio.              |
| `DeliveryApp.Aplicacao`      | Serviços de aplicação e tipos compartilhados de resultado.                |
| `DeliveryApp.Infraestrutura` | EF Core, ASP.NET Core Identity, migrations e acesso ao PostgreSQL.         |
| `DeliveryApp.WebApi`         | Controllers, autenticação JWT, Problem Details, OpenAPI e observabilidade. |

O `DeliveryAppDbContext` herda de `IdentityDbContext` e mantém os dados de identidade e de domínio no mesmo banco. As entidades de perfil seguem o padrão de chave primária compartilhada com o Identity:

| Entidade          | Modelagem da identidade                                                               |
| ----------------- | ------------------------------------------------------------------------------------- |
| `Cliente`         | O `Id` é a chave primária e também a chave estrangeira do usuário, em uma relação 1:1. |
| `Estabelecimento` | O `Id` é a chave primária e também a chave estrangeira do usuário, em uma relação 1:1. |

Assim, `Estabelecimento` não possui um `UsuarioId` separado: seu próprio `Id` identifica tanto o perfil de domínio quanto o usuário correspondente.

## Tecnologias

- .NET 10 e ASP.NET Core Web API;
- ASP.NET Core Identity;
- autenticação JWT Bearer;
- Entity Framework Core 10;
- PostgreSQL com Npgsql;
- FluentResults;
- Serilog com saídas para console e arquivo;
- Swagger/OpenAPI.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0);
- PostgreSQL;
- [EF Core CLI](https://learn.microsoft.com/ef/core/cli/dotnet), somente para gerenciar migrations manualmente.

Para instalar a CLI do EF Core:

```bash
dotnet tool install --global dotnet-ef
```

## Configuração

Em desenvolvimento, o projeto já define em `src/Api/appsettings.Development.json` a conexão local:

```text
Host=localhost;Port=5432;Database=DeliveryAppDb;Username=postgres;Password=postgres
```

Altere-a conforme o seu ambiente ou sobrescreva-a com o Secret Manager:

```bash
dotnet user-secrets set "ConnectionStrings:PostgresEF" "Host=localhost;Port=5432;Database=DeliveryAppDb;Username=postgres;Password=sua-senha" --project src/Api
```

A chave de assinatura do JWT não é armazenada no repositório e precisa ser configurada:

```bash
dotnet user-secrets set "Jwt:Key" "informe-uma-chave-segura-com-pelo-menos-32-caracteres" --project src/Api
```

As demais opções do JWT ficam em `src/Api/appsettings.json`:

| Chave                    | Valor padrão          |
| ------------------------ | --------------------- |
| `Jwt:Issuer`             | `delivery-app-api`    |
| `Jwt:Audience`           | `delivery-app-client` |
| `Jwt:AccessTokenMinutes` | `60`                  |

## Execução

Na raiz da solução, execute:

```bash
dotnet restore DeliveryApp.slnx
dotnet run --project src/Api
```

No ambiente `Development`, as migrations são aplicadas automaticamente na inicialização. A API fica disponível em:

- `https://localhost:7094`;
- `http://localhost:5033`;
- Swagger UI em `/swagger`.

Para atualizar o banco manualmente:

```bash
dotnet ef database update --project src/Infraestrutura --startup-project src/Api
```

## Logs

O Serilog registra eventos no console e grava erros em arquivos diários. Os arquivos ficam em `DeliveryApp/erro*.log` dentro do diretório local de dados da aplicação (`LocalApplicationData`).

## Verificação

Para validar a compilação da solução:

```bash
dotnet build DeliveryApp.slnx
```
