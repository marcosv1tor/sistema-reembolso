# Script PowerShell para executar migrações do Entity Framework
# Este script executa as migrações para todos os microserviços

Write-Host "=== Executando Migrações do Entity Framework ===" -ForegroundColor Green
Write-Host ""

# Função para executar migração
function Run-Migration {
    param(
        [string]$ServiceName,
        [string]$ProjectPath,
        [string]$ContextName
    )
    
    Write-Host "Executando migração para $ServiceName..." -ForegroundColor Yellow
    
    try {
        # Navegar para o diretório do projeto
        Set-Location $ProjectPath
        
        # Verificar se há migrações pendentes
        $pendingMigrations = dotnet ef migrations list --no-build 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Aplicando migrações para $ServiceName..." -ForegroundColor Cyan
            
            # Aplicar migrações
            dotnet ef database update --no-build
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Migrações aplicadas com sucesso para $ServiceName" -ForegroundColor Green
            } else {
                Write-Host "✗ Erro ao aplicar migrações para $ServiceName" -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "⚠ Nenhuma migração encontrada para $ServiceName" -ForegroundColor Yellow
        }
        
        return $true
    }
    catch {
        Write-Host "✗ Erro inesperado ao processar $ServiceName : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Função para criar migração inicial se não existir
function Create-InitialMigration {
    param(
        [string]$ServiceName,
        [string]$ProjectPath,
        [string]$ContextName
    )
    
    Write-Host "Verificando migrações para $ServiceName..." -ForegroundColor Yellow
    
    try {
        Set-Location $ProjectPath
        
        # Verificar se existem migrações
        $migrations = dotnet ef migrations list --no-build 2>$null
        
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($migrations)) {
            Write-Host "Criando migração inicial para $ServiceName..." -ForegroundColor Cyan
            
            # Criar migração inicial
            dotnet ef migrations add InitialCreate --no-build
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Migração inicial criada para $ServiceName" -ForegroundColor Green
                return $true
            } else {
                Write-Host "✗ Erro ao criar migração inicial para $ServiceName" -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "✓ Migrações já existem para $ServiceName" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "✗ Erro ao verificar migrações para $ServiceName : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Verificar se o dotnet ef está instalado
Write-Host "Verificando ferramentas do Entity Framework..." -ForegroundColor Yellow
$efCheck = dotnet ef --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Entity Framework tools não encontrado. Instalando..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Erro ao instalar Entity Framework tools" -ForegroundColor Red
        exit 1
    }
}
Write-Host "✓ Entity Framework tools disponível" -ForegroundColor Green
Write-Host ""

# Definir caminhos dos projetos
$rootPath = Split-Path -Parent $PSScriptRoot
$servicesPath = Join-Path $rootPath "backend\src\Services"

# Lista de serviços para migração
$services = @(
    @{
        Name = "Usuarios"
        Path = Join-Path $servicesPath "Usuarios"
        Context = "UsuariosDbContext"
    },
    @{
        Name = "Funcionarios"
        Path = Join-Path $servicesPath "Funcionarios"
        Context = "FuncionariosDbContext"
    },
    @{
        Name = "SolicitacoesReembolso"
        Path = Join-Path $servicesPath "SolicitacoesReembolso"
        Context = "SolicitacoesDbContext"
    },
    @{
        Name = "Relatorios"
        Path = Join-Path $servicesPath "Relatorios"
        Context = "RelatoriosDbContext"
    }
)

# Compilar todos os projetos primeiro
Write-Host "Compilando todos os projetos..." -ForegroundColor Yellow
Set-Location $rootPath
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Erro ao compilar os projetos" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Projetos compilados com sucesso" -ForegroundColor Green
Write-Host ""

# Executar migrações para cada serviço
$allSuccess = $true

foreach ($service in $services) {
    Write-Host "=== Processando $($service.Name) ===" -ForegroundColor Magenta
    
    # Verificar se o diretório existe
    if (-not (Test-Path $service.Path)) {
        Write-Host "⚠ Diretório não encontrado: $($service.Path)" -ForegroundColor Yellow
        continue
    }
    
    # Criar migração inicial se necessário
    $migrationCreated = Create-InitialMigration -ServiceName $service.Name -ProjectPath $service.Path -ContextName $service.Context
    
    if ($migrationCreated) {
        # Executar migração
        $migrationSuccess = Run-Migration -ServiceName $service.Name -ProjectPath $service.Path -ContextName $service.Context
        
        if (-not $migrationSuccess) {
            $allSuccess = $false
        }
    } else {
        $allSuccess = $false
    }
    
    Write-Host ""
}

# Retornar ao diretório original
Set-Location $rootPath

# Resultado final
Write-Host "=== Resultado Final ===" -ForegroundColor Magenta
if ($allSuccess) {
    Write-Host "✓ Todas as migrações foram executadas com sucesso!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ Algumas migrações falharam. Verifique os logs acima." -ForegroundColor Red
    exit 1
}

# Instruções adicionais
Write-Host ""
Write-Host "=== Instruções Adicionais ===" -ForegroundColor Cyan
Write-Host "1. Certifique-se de que o SQL Server está rodando"
Write-Host "2. Verifique as connection strings nos appsettings.json"
Write-Host "3. Execute 'docker-compose up -d sqlserver' se estiver usando Docker"
Write-Host "4. Para reverter migrações, use: dotnet ef database update <MigrationName>"
Write-Host "5. Para remover última migração: dotnet ef migrations remove"
Write-Host ""