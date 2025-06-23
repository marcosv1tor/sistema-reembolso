# Sistema de Reembolso - Backend

Este é o backend do Sistema de Reembolso, implementado como uma arquitetura de microserviços usando .NET 8.

## Estrutura do Projeto

```
backend/
├── Reembolso.sln                    # Solution principal
├── src/
│   ├── Services/                     # Microserviços
│   │   ├── Usuarios/                 # Serviço de Usuários
│   │   ├── Funcionarios/             # Serviço de Funcionários
│   │   ├── SolicitacoesReembolso/    # Serviço de Solicitações
│   │   └── Relatorios/               # Serviço de Relatórios
│   └── Shared/                       # Código compartilhado
│       └── Reembolso.Shared/         # Biblioteca compartilhada
└── .gitignore                        # Arquivos ignorados pelo Git
```

## Microserviços

### 1. Reembolso.Usuarios.API
- **Porta**: 5001
- **Responsabilidade**: Gerenciamento de usuários, autenticação e autorização
- **Banco**: reembolso_usuarios

### 2. Reembolso.Funcionarios.API
- **Porta**: 5002
- **Responsabilidade**: Gerenciamento de funcionários e departamentos
- **Banco**: reembolso_funcionarios

### 3. Reembolso.SolicitacoesReembolso.API
- **Porta**: 5003
- **Responsabilidade**: Gerenciamento de solicitações de reembolso
- **Banco**: reembolso_solicitacoes

### 4. Reembolso.Relatorios.API
- **Porta**: 5004
- **Responsabilidade**: Geração de relatórios e dashboards
- **Banco**: reembolso_relatorios

### 5. Reembolso.Shared
- **Tipo**: Biblioteca de classes
- **Responsabilidade**: DTOs, modelos, middleware e configurações compartilhadas

## Pré-requisitos

- .NET 8 SDK
- PostgreSQL 15+
- Docker e Docker Compose (para execução completa)
- Visual Studio 2022 ou VS Code

## Como Executar

### Opção 1: Usando Docker Compose (Recomendado)

```bash
# Na raiz do projeto
docker-compose up -d
```

### Opção 2: Executando Localmente

1. **Configurar o banco de dados PostgreSQL**:
   ```bash
   # Execute o script de inicialização
   psql -U postgres -f database/init-db.sql
   ```

2. **Restaurar dependências**:
   ```bash
   cd backend
   dotnet restore
   ```

3. **Executar todos os serviços**:
   ```bash
   # Terminal 1 - Usuários
   cd src/Services/Usuarios/Reembolso.Usuarios.API
   dotnet run
   
   # Terminal 2 - Funcionários
   cd src/Services/Funcionarios/Reembolso.Funcionarios.API
   dotnet run
   
   # Terminal 3 - Solicitações
   cd src/Services/SolicitacoesReembolso/Reembolso.SolicitacoesReembolso.API
   dotnet run
   
   # Terminal 4 - Relatórios
   cd src/Services/Relatorios/Reembolso.Relatorios.API
   dotnet run
   ```

### Opção 3: Usando Visual Studio

1. Abra o arquivo `Reembolso.sln` no Visual Studio
2. Configure múltiplos projetos de inicialização:
   - Clique com o botão direito na Solution
   - Selecione "Set Startup Projects"
   - Escolha "Multiple startup projects"
   - Configure todos os projetos de API para "Start"
3. Pressione F5 para executar

## Configuração

### Variáveis de Ambiente

Cada microserviço utiliza as seguintes variáveis de ambiente:

```bash
# Banco de dados
CONNECTION_STRING=Host=localhost;Database=nome_do_banco;Username=reembolso_user;Password=senha123

# JWT
JWT_SECRET=sua_chave_secreta_jwt_aqui
JWT_ISSUER=ReembolsoSystem
JWT_AUDIENCE=ReembolsoAPI
JWT_EXPIRATION_HOURS=24

# CORS
CORS_ORIGINS=http://localhost:3000,http://localhost:8080

# Logging
LOG_LEVEL=Information
```

### Arquivos de Configuração

Cada serviço possui:
- `appsettings.json`: Configurações de produção
- `appsettings.Development.json`: Configurações de desenvolvimento

## Endpoints Principais

### API Gateway (Nginx) - http://localhost:8080

- `/api/usuarios/*` → Serviço de Usuários
- `/api/funcionarios/*` → Serviço de Funcionários
- `/api/solicitacoes/*` → Serviço de Solicitações
- `/api/relatorios/*` → Serviço de Relatórios

### Documentação da API

Cada serviço expõe documentação Swagger:

- Usuários: http://localhost:5001/swagger
- Funcionários: http://localhost:5002/swagger
- Solicitações: http://localhost:5003/swagger
- Relatórios: http://localhost:5004/swagger

## Desenvolvimento

### Estrutura de Pastas por Serviço

```
Reembolso.[Nome].API/
├── Controllers/          # Controladores da API
├── DTOs/                # Data Transfer Objects
├── Data/                # Contexto do Entity Framework
├── Models/              # Modelos de domínio
├── Services/            # Lógica de negócio
├── Mappings/            # Mapeamentos AutoMapper
├── Program.cs           # Ponto de entrada
├── Dockerfile           # Configuração Docker
└── appsettings.json     # Configurações
```

### Comandos Úteis

```bash
# Compilar toda a solution
dotnet build

# Executar testes
dotnet test

# Limpar build
dotnet clean

# Restaurar pacotes
dotnet restore

# Adicionar migração (exemplo para Usuários)
cd src/Services/Usuarios/Reembolso.Usuarios.API
dotnet ef migrations add NomeDaMigracao

# Aplicar migrações
dotnet ef database update
```

## Monitoramento

### Health Checks

Todos os serviços expõem endpoints de health check:
- `/health` - Status geral
- `/health/ready` - Pronto para receber tráfego
- `/health/live` - Serviço está vivo

### Logs

- **Desenvolvimento**: Console e Debug
- **Produção**: Elasticsearch (via Docker Compose)
- **Formato**: JSON estruturado

### Métricas

- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3001 (admin/admin)

## Troubleshooting

### Problemas Comuns

1. **Erro de conexão com banco**:
   - Verifique se o PostgreSQL está rodando
   - Confirme as credenciais no connection string

2. **Porta já em uso**:
   - Altere as portas nos arquivos `appsettings.json`
   - Ou pare outros serviços que estejam usando as portas

3. **Erro de CORS**:
   - Verifique a configuração `CORS_ORIGINS`
   - Confirme se o frontend está na lista de origens permitidas

4. **JWT inválido**:
   - Verifique se todos os serviços estão usando a mesma `JWT_SECRET`
   - Confirme se o token não expirou

### Logs de Debug

Para habilitar logs detalhados:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-feature`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova feature'`)
4. Push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.