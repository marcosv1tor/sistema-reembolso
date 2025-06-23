# Sistema de Reembolso - Microserviços

## 📋 Visão Geral

Sistema completo de gerenciamento de reembolsos desenvolvido em arquitetura de microserviços com .NET 8, utilizando as melhores práticas de desenvolvimento, segurança e monitoramento.

## 🏗️ Arquitetura

### Microserviços

1. **API de Usuários** (`Reembolso.Usuarios.API`)
   - Gerenciamento de usuários e autenticação
   - JWT Token authentication
   - Controle de perfis e permissões

2. **API de Funcionários** (`Reembolso.Funcionarios.API`)
   - Cadastro e gerenciamento de funcionários
   - Estrutura organizacional
   - Aprovadores e hierarquia

3. **API de Solicitações** (`Reembolso.Solicitacoes.API`)
   - Criação e gerenciamento de solicitações de reembolso
   - Workflow de aprovação
   - Upload de comprovantes

4. **API de Relatórios** (`Reembolso.Relatorios.API`)
   - Geração de relatórios em PDF e Excel
   - Estatísticas e dashboards
   - Processamento em background

### Componentes de Infraestrutura

- **API Gateway** (Nginx)
- **Banco de Dados** (SQL Server)
- **Cache** (Redis - opcional)
- **Logs Centralizados** (Elasticsearch + Kibana - opcional)
- **Monitoramento** (Prometheus + Grafana - opcional)

## 🚀 Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **Entity Framework Core** - ORM
- **AutoMapper** - Mapeamento de objetos
- **JWT Bearer** - Autenticação
- **Swagger/OpenAPI** - Documentação da API
- **Serilog** - Logging estruturado
- **iTextSharp** - Geração de PDFs
- **EPPlus** - Geração de Excel
- **Docker** - Containerização
- **Nginx** - API Gateway e Load Balancer
- **SQL Server** - Banco de dados
- **Redis** - Cache (opcional)
- **Elasticsearch** - Busca e logs (opcional)
- **Prometheus** - Métricas (opcional)
- **Grafana** - Dashboards (opcional)

## 📁 Estrutura do Projeto

```
backend/
├── src/
│   ├── Shared/                     # Componentes compartilhados
│   │   ├── DTOs/                   # Data Transfer Objects
│   │   ├── Interfaces/             # Interfaces dos serviços
│   │   ├── Services/               # Serviços compartilhados
│   │   ├── Middleware/             # Middlewares customizados
│   │   └── Reembolso.Shared.csproj
│   ├── Usuarios/                   # Microserviço de Usuários
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Data/
│   │   └── Reembolso.Usuarios.API.csproj
│   ├── Funcionarios/               # Microserviço de Funcionários
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Data/
│   │   └── Reembolso.Funcionarios.API.csproj
│   ├── Solicitacoes/               # Microserviço de Solicitações
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Data/
│   │   └── Reembolso.Solicitacoes.API.csproj
│   └── Relatorios/                 # Microserviço de Relatórios
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Data/
│       └── Reembolso.Relatorios.API.csproj
├── docker-compose.yml              # Orquestração dos serviços
├── nginx.conf                      # Configuração do API Gateway
├── prometheus.yml                  # Configuração do Prometheus
├── init-db.sql                     # Script de inicialização do BD
├── run-migrations.ps1              # Script para executar migrações
└── setup-environment.ps1           # Script de configuração do ambiente
```

## 🔧 Configuração e Instalação

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [SQL Server](https://www.microsoft.com/sql-server) (ou usar container)
- [PowerShell](https://docs.microsoft.com/powershell/) (para scripts de automação)

### Instalação Rápida

1. **Clone o repositório**
   ```bash
   git clone <repository-url>
   cd teste2
   ```

2. **Execute o script de configuração**
   ```powershell
   .\setup-environment.ps1 -Environment Development -Action Setup
   ```

3. **Acesse as aplicações**
   - API Gateway: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger
   - Grafana: http://localhost:3000 (admin/admin)
   - Kibana: http://localhost:5601

### Instalação Manual

1. **Configurar banco de dados**
   ```powershell
   # Iniciar SQL Server via Docker
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
   
   # Executar script de inicialização
   sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -i init-db.sql
   ```

2. **Executar migrações**
   ```powershell
   .\run-migrations.ps1
   ```

3. **Compilar projetos**
   ```bash
   dotnet build backend/src/Usuarios/Reembolso.Usuarios.API.csproj
   dotnet build backend/src/Funcionarios/Reembolso.Funcionarios.API.csproj
   dotnet build backend/src/Solicitacoes/Reembolso.Solicitacoes.API.csproj
   dotnet build backend/src/Relatorios/Reembolso.Relatorios.API.csproj
   ```

4. **Executar com Docker Compose**
   ```bash
   docker-compose up -d
   ```

## 🔐 Autenticação e Autorização

### JWT Token

O sistema utiliza JWT Bearer tokens para autenticação. Para obter um token:

```bash
POST /api/auth/login
{
  "email": "usuario@exemplo.com",
  "senha": "senha123"
}
```

### Perfis de Usuário

- **Admin**: Acesso total ao sistema
- **Gestor**: Aprovação de solicitações e relatórios
- **Funcionario**: Criação de solicitações
- **Financeiro**: Processamento de pagamentos

## 📊 Monitoramento e Logs

### Health Checks

Todos os microserviços possuem endpoints de health check:

- `/health` - Status geral
- `/health/ready` - Pronto para receber tráfego
- `/health/live` - Aplicação está viva

### Logs Centralizados

Os logs são enviados para Elasticsearch e podem ser visualizados no Kibana:

- **Logs de aplicação**: Informações, avisos e erros
- **Logs de auditoria**: Ações dos usuários
- **Logs de performance**: Métricas de tempo de resposta
- **Logs de segurança**: Tentativas de acesso não autorizado

### Métricas

Prometheus coleta métricas dos serviços:

- **Métricas de aplicação**: Requests/segundo, latência, erros
- **Métricas de sistema**: CPU, memória, disco
- **Métricas de negócio**: Solicitações criadas, aprovadas, rejeitadas

## 🔄 CI/CD e Deploy

### Scripts de Automação

- `setup-environment.ps1`: Configuração completa do ambiente
- `run-migrations.ps1`: Execução de migrações do Entity Framework
- `docker-compose.yml`: Orquestração de todos os serviços

### Ambientes

- **Development**: Ambiente local com todas as funcionalidades
- **Staging**: Ambiente de homologação
- **Production**: Ambiente de produção

## 📝 API Documentation

### Swagger UI

Cada microserviço possui documentação Swagger acessível em:

- Usuários: http://localhost:8080/usuarios/swagger
- Funcionários: http://localhost:8080/funcionarios/swagger
- Solicitações: http://localhost:8080/solicitacoes/swagger
- Relatórios: http://localhost:8080/relatorios/swagger

### Endpoints Principais

#### Usuários
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Registro
- `GET /api/usuarios` - Listar usuários
- `PUT /api/usuarios/{id}` - Atualizar usuário

#### Funcionários
- `GET /api/funcionarios` - Listar funcionários
- `POST /api/funcionarios` - Criar funcionário
- `PUT /api/funcionarios/{id}` - Atualizar funcionário
- `DELETE /api/funcionarios/{id}` - Excluir funcionário

#### Solicitações
- `GET /api/solicitacoes` - Listar solicitações
- `POST /api/solicitacoes` - Criar solicitação
- `PUT /api/solicitacoes/{id}/aprovar` - Aprovar solicitação
- `PUT /api/solicitacoes/{id}/rejeitar` - Rejeitar solicitação

#### Relatórios
- `GET /api/relatorios` - Listar relatórios
- `POST /api/relatorios/gerar` - Gerar relatório
- `GET /api/relatorios/{id}/download` - Download do relatório
- `GET /api/relatorios/estatisticas` - Estatísticas

## 🧪 Testes

### Executar Testes

```bash
# Testes unitários
dotnet test

# Testes de integração
dotnet test --filter Category=Integration

# Cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

### Testes de Carga

Utilize ferramentas como k6 ou Artillery para testes de carga:

```bash
# Exemplo com curl para teste básico
curl -X GET http://localhost:8080/api/usuarios/health
```

## 🔧 Troubleshooting

### Problemas Comuns

1. **Erro de conexão com banco de dados**
   - Verificar se SQL Server está rodando
   - Validar connection string
   - Executar migrações

2. **Erro 401 Unauthorized**
   - Verificar se token JWT é válido
   - Confirmar configuração de autenticação

3. **Erro 500 Internal Server Error**
   - Verificar logs da aplicação
   - Validar configurações do appsettings.json

### Logs e Debugging

```bash
# Ver logs dos containers
docker-compose logs -f [service-name]

# Acessar container
docker exec -it [container-name] /bin/bash

# Verificar status dos serviços
docker-compose ps
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 🔄 Changelog

### v1.0.0 (2024-01-XX)
- ✅ Implementação inicial dos 4 microserviços
- ✅ API Gateway com Nginx
- ✅ Autenticação JWT
- ✅ Geração de relatórios PDF/Excel
- ✅ Upload de arquivos
- ✅ Logs centralizados
- ✅ Health checks
- ✅ Monitoramento com Prometheus/Grafana
- ✅ Containerização com Docker
- ✅ Scripts de automação

---

**Desenvolvido com ❤️ por mim**
