-- Script de inicialização do banco de dados PostgreSQL para o sistema de reembolso
-- Este script cria os bancos de dados para cada microserviço

-- Criar banco de dados para Usuários
SELECT 'CREATE DATABASE reembolso_usuarios'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'reembolso_usuarios')\gexec

-- Criar banco de dados para Funcionários
SELECT 'CREATE DATABASE reembolso_funcionarios'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'reembolso_funcionarios')\gexec

-- Criar banco de dados para Solicitações de Reembolso
SELECT 'CREATE DATABASE reembolso_solicitacoes'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'reembolso_solicitacoes')\gexec

-- Criar banco de dados para Relatórios
SELECT 'CREATE DATABASE reembolso_relatorios'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'reembolso_relatorios')\gexec

-- Criar usuário para os microserviços
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'reembolso_user') THEN
db:
  image: postgres
  ports:
    - "5432:5432"
      CREATE ROLE reembolso_user LOGIN PASSWORD 'ReembolsoPass123!';
   END IF;
END
$do$;

-- Conectar ao banco ReembolsoUsuarios e configurar permissões
\c reembolso_usuarios;
GRANT ALL PRIVILEGES ON DATABASE reembolso_usuarios TO reembolso_user;
GRANT ALL ON SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO reembolso_user;

-- Conectar ao banco ReembolsoFuncionarios e configurar permissões
\c reembolso_funcionarios;
GRANT ALL PRIVILEGES ON DATABASE reembolso_funcionarios TO reembolso_user;
GRANT ALL ON SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO reembolso_user;

-- Conectar ao banco ReembolsoSolicitacoes e configurar permissões
\c reembolso_solicitacoes;
GRANT ALL PRIVILEGES ON DATABASE reembolso_solicitacoes TO reembolso_user;
GRANT ALL ON SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO reembolso_user;

-- Conectar ao banco ReembolsoRelatorios e configurar permissões
\c reembolso_relatorios;
GRANT ALL PRIVILEGES ON DATABASE reembolso_relatorios TO reembolso_user;
GRANT ALL ON SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO reembolso_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO reembolso_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO reembolso_user;

-- Configurações de performance e segurança
\c postgres;

-- Configurar parâmetros de performance para os bancos
ALTER DATABASE reembolso_usuarios SET default_transaction_isolation = 'read committed';
ALTER DATABASE reembolso_funcionarios SET default_transaction_isolation = 'read committed';
ALTER DATABASE reembolso_solicitacoes SET default_transaction_isolation = 'read committed';
ALTER DATABASE reembolso_relatorios SET default_transaction_isolation = 'read committed';

-- Configurar timezone
ALTER DATABASE reembolso_usuarios SET timezone = 'America/Sao_Paulo';
ALTER DATABASE reembolso_funcionarios SET timezone = 'America/Sao_Paulo';
ALTER DATABASE reembolso_solicitacoes SET timezone = 'America/Sao_Paulo';
ALTER DATABASE reembolso_relatorios SET timezone = 'America/Sao_Paulo';

-- Configurar encoding
ALTER DATABASE reembolso_usuarios SET client_encoding = 'UTF8';
ALTER DATABASE reembolso_funcionarios SET client_encoding = 'UTF8';
ALTER DATABASE reembolso_solicitacoes SET client_encoding = 'UTF8';
ALTER DATABASE reembolso_relatorios SET client_encoding = 'UTF8';

\echo 'Configurações de banco de dados PostgreSQL aplicadas com sucesso.'
\echo 'Inicialização do banco de dados concluída.'