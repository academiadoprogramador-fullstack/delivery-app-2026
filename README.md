# DeliveryApp

Desenvolvido durante o curso Fullstack da [Academia do Programador 2026](https://www.academiadoprogramador.net).

API para gerenciamento de clientes e estabelecimentos de uma plataforma de pedidos e entregas, com autenticação JWT, isolamento de dados por usuário e persistência em SQL Server.

## Especificação funcional

Cada módulo apresenta primeiro as entidades e suas propriedades. Em seguida, são descritas as regras de negócio e os comportamentos implementados.

### 1. Módulo de usuários e autenticação

A autenticação utiliza ASP.NET Core Identity e separa os acessos conforme o tipo de perfil associado ao usuário.

#### Tipos de usuário

| Tipo              | Descrição                                                      |
| ----------------- | -------------------------------------------------------------- |
| `Cliente`         | Consumidor autorizado a acessar os dados do próprio perfil.    |
| `Estabelecimento` | Parceiro autorizado a administrar o estabelecimento vinculado. |

#### Regras de negócio

##### Cadastro

- O email deve ser único.
- A senha deve possuir no mínimo oito caracteres, um dígito e um caractere não alfanumérico.
- Cada identidade deve receber o papel correspondente ao fluxo de cadastro utilizado.
- As credenciais e as chaves de assinatura não são armazenadas no código-fonte.

##### Autenticação

- Clientes e estabelecimentos possuem fluxos de login separados.
- O token JWT contém o identificador do usuário, o email e o papel de acesso.
- O token de autenticação possui duração configurável.
- Após cinco tentativas malsucedidas, a conta é bloqueada por cinco minutos.
- Credenciais inválidas retornam uma resposta genérica, sem revelar qual dado está incorreto.

##### Autorização

- Os endpoints são protegidos por autenticação por padrão.
- Endpoints públicos são identificados explicitamente.
- Operações administrativas validam o vínculo entre o usuário autenticado e o perfil de domínio.
- Um usuário não pode consultar ou alterar dados privados pertencentes a outro usuário.

### 2. Módulo de clientes

#### Entidade `Cliente`

| Propriedade | Descrição                                           |
| ----------- | --------------------------------------------------- |
| `ID`        | Identificador do cliente e da identidade associada. |
| `Nome`      | Nome do cliente.                                    |
| `CPF`       | Documento normalizado do cliente.                   |

#### Regras de negócio

##### Cadastro

- O nome deve possuir entre 2 e 100 caracteres.
- O CPF deve possuir exatamente 11 dígitos.
- Pontos, traços e espaços do CPF são removidos antes da validação.
- Não é permitido cadastrar dois clientes com o mesmo CPF.
- O email deve ser único entre todos os usuários.
- O cliente e sua identidade são criados com o mesmo identificador.

##### Autenticação

- O login é realizado por email e senha no fluxo próprio de clientes.
- O usuário deve possuir um perfil de cliente e o papel `Cliente`.
- A resposta contém o identificador do cliente e o token JWT.

##### Consulta

- A listagem retorna somente o cliente associado ao usuário autenticado.
- A consulta por identificador retorna apenas o próprio perfil.
- A tentativa de consultar outro cliente retorna `404 Not Found`, sem revelar a existência do registro.

#### Endpoints

| Método | Rota                        | Acesso  | Descrição                         |
| ------ | --------------------------- | ------- | --------------------------------- |
| `POST` | `/api/clientes/cadastro`    | Público | Cadastra e autentica um cliente.  |
| `POST` | `/api/clientes/login`       | Público | Autentica um cliente.             |
| `GET`  | `/api/clientes`             | Cliente | Lista o próprio perfil.           |
| `GET`  | `/api/clientes/{clienteId}` | Cliente | Consulta o próprio perfil por ID. |

### 3. Módulo de estabelecimentos

#### Entidade `Estabelecimento`

| Propriedade         | Descrição                                         |
| ------------------- | ------------------------------------------------- |
| `ID`                | Identificador do estabelecimento.                 |
| `UsuarioId`         | Identificador único da identidade associada.      |
| `NomeComercial`     | Nome utilizado comercialmente.                    |
| `Documento`         | CPF ou CNPJ normalizado.                          |
| `Endereco`          | Endereço do estabelecimento.                      |
| `Telefone`          | Telefone normalizado para contato.                |
| `HorarioAbertura`   | Início do período diário de atendimento.          |
| `HorarioFechamento` | Final do período diário de atendimento.           |
| `AreaAtendimento`   | Descrição das regiões atendidas.                  |
| `Ativo`             | Indica se o estabelecimento aceita novos pedidos. |

#### Regras de negócio

##### Cadastro

- O nome comercial deve possuir entre 2 e 100 caracteres.
- O documento deve possuir 11 ou 14 dígitos.
- O endereço deve possuir entre 5 e 250 caracteres.
- O telefone deve possuir 10 ou 11 dígitos.
- A área de atendimento deve possuir entre 2 e 150 caracteres.
- O horário de abertura deve ser diferente do horário de fechamento.
- Documento e telefone são normalizados antes da validação.
- Não é permitido manter dois estabelecimentos ativos com o mesmo documento.
- Cada usuário pode estar vinculado a apenas um estabelecimento.
- Novos estabelecimentos são cadastrados como ativos.

##### Autenticação

- O login é realizado por email e senha no fluxo próprio de estabelecimentos.
- O usuário deve possuir o papel `Estabelecimento`.
- A resposta contém o identificador do estabelecimento e o token JWT.

##### Edição

- Somente o usuário vinculado pode editar o estabelecimento.
- As mesmas validações do cadastro são aplicadas durante a edição.
- A edição não pode gerar duplicidade de documento entre estabelecimentos ativos.

##### Ativação e desativação

- Somente o usuário vinculado pode alterar o status do estabelecimento.
- Um estabelecimento inativo não aparece entre os disponíveis para pedidos.
- Um estabelecimento não pode ser ativado quando outro registro ativo utiliza o mesmo documento.

##### Disponibilidade

- Um estabelecimento está disponível quando está ativo e o horário atual está dentro do período de atendimento.
- Horários que atravessam a meia-noite são suportados.
- A avaliação dos horários utiliza UTC.
- Clientes consultam somente estabelecimentos disponíveis.
- O proprietário pode consultar o próprio estabelecimento mesmo quando estiver fechado ou inativo.

#### Endpoints

| Método  | Rota                                                  | Acesso                     | Descrição                                |
| ------- | ----------------------------------------------------- | -------------------------- | ---------------------------------------- |
| `POST`  | `/api/estabelecimentos/cadastro`                      | Público                    | Cadastra e autentica um estabelecimento. |
| `POST`  | `/api/estabelecimentos/login`                         | Público                    | Autentica um estabelecimento.            |
| `GET`   | `/api/estabelecimentos`                               | Cliente ou Estabelecimento | Lista estabelecimentos disponíveis.      |
| `GET`   | `/api/estabelecimentos/disponiveis`                   | Cliente ou Estabelecimento | Lista estabelecimentos disponíveis.      |
| `GET`   | `/api/estabelecimentos/{estabelecimentoId}`           | Cliente ou Estabelecimento | Consulta um estabelecimento acessível.   |
| `PUT`   | `/api/estabelecimentos/{estabelecimentoId}`           | Estabelecimento            | Edita o estabelecimento vinculado.       |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/ativar`    | Estabelecimento            | Ativa o estabelecimento vinculado.       |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/desativar` | Estabelecimento            | Desativa o estabelecimento vinculado.    |

## Arquitetura

A solução está organizada em camadas:

| Projeto                      | Responsabilidade                                      |
| ---------------------------- | ----------------------------------------------------- |
| `DeliveryApp.Dominio`        | Entidades, validações e regras de domínio.            |
| `DeliveryApp.Aplicacao`      | Casos de uso e handlers MediatR.                      |
| `DeliveryApp.Infraestrutura` | Entity Framework Core, Identity e acesso aos dados.   |
| `DeliveryApp.WebApi`         | Endpoints HTTP, autenticação, Swagger e configuração. |
| `DeliveryApp.Tests`          | Testes unitários e de integração da API.              |

As respostas de erro seguem o padrão [Problem Details](https://www.rfc-editor.org/rfc/rfc9457), incluindo um identificador de rastreamento.

## Tecnologias

- .NET 10;
- ASP.NET Core Web API;
- ASP.NET Core Identity;
- Entity Framework Core;
- SQL Server;
- MediatR;
- FluentResults;
- JWT Bearer Authentication;
- Serilog;
- New Relic Logs;
- Swagger/OpenAPI;
- xUnit.

## Como utilizar

1. Clone o repositório ou baixe o código-fonte.
2. Abra o terminal ou o prompt de comando e navegue até a pasta raiz da solução.
3. Restaure as dependências:

   ```bash
   dotnet restore
   ```

4. Configure os dados sensíveis com o Secret Manager:

   ```bash
   dotnet user-secrets set "ConnectionStrings:SqlServerEF" "Server=localhost;Database=DeliveryApp;Trusted_Connection=True;TrustServerCertificate=True" --project src/Api
   dotnet user-secrets set "Jwt:Key" "informe-uma-chave-segura-com-pelo-menos-32-caracteres" --project src/Api
   dotnet user-secrets set "LuckyPenny:LicenseKey" "informe-a-chave-de-licenca-do-mediatr" --project src/Api
   ```

5. Execute a API:

   ```bash
   dotnet run --project src/Api
   ```

No ambiente de desenvolvimento, as migrations são aplicadas automaticamente durante a inicialização. A documentação Swagger fica disponível em `https://localhost:7193/swagger` ou `http://localhost:5164/swagger`.

### New Relic

A integração com o New Relic fica desabilitada em desenvolvimento. Para habilitá-la, configure:

```bash
dotnet user-secrets set "NewRelic:Enabled" "true" --project src/Api
dotnet user-secrets set "NewRelic:LicenseKey" "informe-a-chave-do-new-relic" --project src/Api
```

Os erros também são registrados diariamente no diretório local de dados da aplicação, dentro da pasta `DeliveryApp`.

## Banco de dados

Para criar ou atualizar o banco manualmente, execute:

```bash
dotnet ef database update --project src/Infraestrutura --startup-project src/Api
```

## Testes

Execute todos os testes a partir da raiz da solução:

```bash
dotnet test DeliveryApp.slnx
```

Os testes de integração utilizam o provedor InMemory do Entity Framework Core e não dependem de uma instância externa do SQL Server.

## Requisitos

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0);
- SQL Server;
- [EF Core CLI](https://learn.microsoft.com/ef/core/cli/dotnet), para gerenciamento manual das migrations;
- chave de licença válida do MediatR.
