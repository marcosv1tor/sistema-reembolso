# Script PowerShell para configurar e executar o ambiente completo
# Este script configura e inicia todos os serviços do sistema de reembolso

param(
    [switch]$SkipBuild,
    [switch]$SkipMigrations,
    [switch]$Development,
    [switch]$Production,
    [switch]$StopServices,
    [switch]$CleanUp
)

# Configurações
$ErrorActionPreference = "Stop"
$rootPath = Split-Path -Parent $PSScriptRoot

# Função para exibir banner
function Show-Banner {
    Write-Host "" -ForegroundColor Green
    Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║                    SISTEMA DE REEMBOLSO                     ║" -ForegroundColor Green
    Write-Host "║                   Setup Environment                         ║" -ForegroundColor Green
    Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host "" -ForegroundColor Green
}

# Função para log com timestamp
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Level) {
        "ERROR" { "Red" }
        "WARN" { "Yellow" }
        "SUCCESS" { "Green" }
        "INFO" { "Cyan" }
        default { "White" }
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

# Função para verificar pré-requisitos
function Test-Prerequisites {
    Write-Log "Verificando pré-requisitos..." "INFO"
    
    # Verificar Docker
    try {
        $dockerVersion = docker --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ Docker encontrado: $dockerVersion" "SUCCESS"
        } else {
            throw "Docker não encontrado"
        }
    }
    catch {
        Write-Log "✗ Docker não está instalado ou não está no PATH" "ERROR"
        return $false
    }
    
    # Verificar Docker Compose
    try {
        $composeVersion = docker-compose --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ Docker Compose encontrado: $composeVersion" "SUCCESS"
        } else {
            throw "Docker Compose não encontrado"
        }
    }
    catch {
        Write-Log "✗ Docker Compose não está instalado" "ERROR"
        return $false
    }
    
    # Verificar .NET SDK
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Log "✓ .NET SDK encontrado: $dotnetVersion" "SUCCESS"
        } else {
            throw ".NET SDK não encontrado"
        }
    }
    catch {
        Write-Log "✗ .NET SDK não está instalado" "ERROR"
        return $false
    }
    
    return $true
}

# Função para parar serviços
function Stop-Services {
    Write-Log "Parando todos os serviços..." "INFO"
    
    Set-Location $rootPath
    
    try {
        docker-compose down --remove-orphans
        Write-Log "✓ Serviços parados com sucesso" "SUCCESS"
    }
    catch {
        Write-Log "✗ Erro ao parar serviços: $($_.Exception.Message)" "ERROR"
    }
}

# Função para limpeza
function Invoke-CleanUp {
    Write-Log "Executando limpeza completa..." "INFO"
    
    Set-Location $rootPath
    
    try {
        # Parar e remover containers
        docker-compose down --remove-orphans --volumes
        
        # Remover imagens do projeto
        $images = docker images --filter "reference=*reembolso*" -q
        if ($images) {
            docker rmi $images -f
        }
        
        # Limpar volumes órfãos
        docker volume prune -f
        
        # Limpar redes órfãs
        docker network prune -f
        
        Write-Log "✓ Limpeza concluída com sucesso" "SUCCESS"
    }
    catch {
        Write-Log "✗ Erro durante limpeza: $($_.Exception.Message)" "ERROR"
    }
}

# Função para compilar projetos
function Build-Projects {
    Write-Log "Compilando projetos..." "INFO"
    
    Set-Location $rootPath
    
    try {
        # Restaurar dependências
        Write-Log "Restaurando dependências..." "INFO"
        dotnet restore
        
        if ($LASTEXITCODE -ne 0) {
            throw "Erro ao restaurar dependências"
        }
        
        # Compilar solução
        Write-Log "Compilando solução..." "INFO"
        dotnet build --configuration Release --no-restore
        
        if ($LASTEXITCODE -ne 0) {
            throw "Erro ao compilar projetos"
        }
        
        Write-Log "✓ Projetos compilados com sucesso" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "✗ Erro ao compilar projetos: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

# Função para executar migrações
function Run-Migrations {
    Write-Log "Executando migrações do banco de dados..." "INFO"
    
    $migrationScript = Join-Path $PSScriptRoot "run-migrations.ps1"
    
    if (Test-Path $migrationScript) {
        try {
            & $migrationScript
            if ($LASTEXITCODE -eq 0) {
                Write-Log "✓ Migrações executadas com sucesso" "SUCCESS"
                return $true
            } else {
                throw "Script de migração retornou código de erro $LASTEXITCODE"
            }
        }
        catch {
            Write-Log "✗ Erro ao executar migrações: $($_.Exception.Message)" "ERROR"
            return $false
        }
    } else {
        Write-Log "⚠ Script de migração não encontrado" "WARN"
        return $false
    }
}

# Função para iniciar serviços
function Start-Services {
    param([string]$Environment = "development")
    
    Write-Log "Iniciando serviços em modo $Environment..." "INFO"
    
    Set-Location $rootPath
    
    try {
        # Definir arquivo de compose baseado no ambiente
        $composeFile = "docker-compose.yml"
        if ($Environment -eq "production") {
            $composeFile = "docker-compose.prod.yml"
        }
        
        # Verificar se o arquivo existe
        if (-not (Test-Path $composeFile)) {
            Write-Log "⚠ Arquivo $composeFile não encontrado, usando docker-compose.yml" "WARN"
            $composeFile = "docker-compose.yml"
        }
        
        # Iniciar serviços de infraestrutura primeiro
        Write-Log "Iniciando serviços de infraestrutura..." "INFO"
        docker-compose -f $composeFile up -d sqlserver redis elasticsearch
        
        # Aguardar serviços ficarem prontos
        Write-Log "Aguardando serviços de infraestrutura ficarem prontos..." "INFO"
        Start-Sleep -Seconds 30
        
        # Iniciar APIs
        Write-Log "Iniciando APIs..." "INFO"
        docker-compose -f $composeFile up -d usuarios-api funcionarios-api solicitacoes-api relatorios-api
        
        # Aguardar APIs ficarem prontas
        Write-Log "Aguardando APIs ficarem prontas..." "INFO"
        Start-Sleep -Seconds 20
        
        # Iniciar API Gateway
        Write-Log "Iniciando API Gateway..." "INFO"
        docker-compose -f $composeFile up -d api-gateway
        
        # Iniciar serviços de monitoramento (opcional)
        if ($Environment -eq "production") {
            Write-Log "Iniciando serviços de monitoramento..." "INFO"
            docker-compose -f $composeFile up -d prometheus grafana kibana
        }
        
        Write-Log "✓ Serviços iniciados com sucesso" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "✗ Erro ao iniciar serviços: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

# Função para verificar saúde dos serviços
function Test-ServicesHealth {
    Write-Log "Verificando saúde dos serviços..." "INFO"
    
    $services = @(
        @{ Name = "API Gateway"; Url = "http://localhost/health" },
        @{ Name = "Usuarios API"; Url = "http://localhost/api/usuarios/health" },
        @{ Name = "Funcionarios API"; Url = "http://localhost/api/funcionarios/health" },
        @{ Name = "Solicitacoes API"; Url = "http://localhost/api/solicitacoes/health" },
        @{ Name = "Relatorios API"; Url = "http://localhost/api/relatorios/health" }
    )
    
    $allHealthy = $true
    
    foreach ($service in $services) {
        try {
            $response = Invoke-WebRequest -Uri $service.Url -TimeoutSec 10 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-Log "✓ $($service.Name) está saudável" "SUCCESS"
            } else {
                Write-Log "⚠ $($service.Name) retornou status $($response.StatusCode)" "WARN"
                $allHealthy = $false
            }
        }
        catch {
            Write-Log "✗ $($service.Name) não está respondendo" "ERROR"
            $allHealthy = $false
        }
    }
    
    return $allHealthy
}

# Função para exibir informações dos serviços
function Show-ServicesInfo {
    Write-Log "Informações dos serviços:" "INFO"
    Write-Host ""
    Write-Host "🌐 API Gateway: http://localhost" -ForegroundColor Green
    Write-Host "👥 Usuarios API: http://localhost/api/usuarios" -ForegroundColor Green
    Write-Host "👨‍💼 Funcionarios API: http://localhost/api/funcionarios" -ForegroundColor Green
    Write-Host "💰 Solicitacoes API: http://localhost/api/solicitacoes" -ForegroundColor Green
    Write-Host "📊 Relatorios API: http://localhost/api/relatorios" -ForegroundColor Green
    Write-Host ""
    Write-Host "📚 Documentação Swagger:" -ForegroundColor Cyan
    Write-Host "   - Usuarios: http://localhost/swagger/usuarios" -ForegroundColor Cyan
    Write-Host "   - Funcionarios: http://localhost/swagger/funcionarios" -ForegroundColor Cyan
    Write-Host "   - Solicitacoes: http://localhost/swagger/solicitacoes" -ForegroundColor Cyan
    Write-Host "   - Relatorios: http://localhost/swagger/relatorios" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🔍 Monitoramento:" -ForegroundColor Yellow
    Write-Host "   - Kibana: http://localhost:5601" -ForegroundColor Yellow
    Write-Host "   - Grafana: http://localhost:3000 (admin/admin)" -ForegroundColor Yellow
    Write-Host "   - Prometheus: http://localhost:9090" -ForegroundColor Yellow
    Write-Host ""
}

# Função principal
function Main {
    Show-Banner
    
    # Processar parâmetros
    if ($StopServices) {
        Stop-Services
        return
    }
    
    if ($CleanUp) {
        Invoke-CleanUp
        return
    }
    
    # Verificar pré-requisitos
    if (-not (Test-Prerequisites)) {
        Write-Log "Pré-requisitos não atendidos. Abortando." "ERROR"
        exit 1
    }
    
    # Determinar ambiente
    $environment = "development"
    if ($Production) {
        $environment = "production"
    }
    
    Write-Log "Configurando ambiente: $environment" "INFO"
    
    # Compilar projetos
    if (-not $SkipBuild) {
        if (-not (Build-Projects)) {
            Write-Log "Falha na compilação. Abortando." "ERROR"
            exit 1
        }
    } else {
        Write-Log "Pulando compilação (--SkipBuild especificado)" "WARN"
    }
    
    # Iniciar serviços
    if (-not (Start-Services -Environment $environment)) {
        Write-Log "Falha ao iniciar serviços. Abortando." "ERROR"
        exit 1
    }
    
    # Executar migrações
    if (-not $SkipMigrations) {
        Write-Log "Aguardando banco de dados ficar pronto..." "INFO"
        Start-Sleep -Seconds 15
        
        if (-not (Run-Migrations)) {
            Write-Log "Falha nas migrações. Continuando mesmo assim..." "WARN"
        }
    } else {
        Write-Log "Pulando migrações (--SkipMigrations especificado)" "WARN"
    }
    
    # Verificar saúde dos serviços
    Write-Log "Aguardando serviços ficarem prontos..." "INFO"
    Start-Sleep -Seconds 30
    
    $maxRetries = 5
    $retryCount = 0
    $servicesHealthy = $false
    
    while ($retryCount -lt $maxRetries -and -not $servicesHealthy) {
        $retryCount++
        Write-Log "Tentativa $retryCount de $maxRetries para verificar saúde dos serviços..." "INFO"
        
        $servicesHealthy = Test-ServicesHealth
        
        if (-not $servicesHealthy -and $retryCount -lt $maxRetries) {
            Write-Log "Aguardando 30 segundos antes da próxima tentativa..." "INFO"
            Start-Sleep -Seconds 30
        }
    }
    
    if ($servicesHealthy) {
        Write-Log "✓ Todos os serviços estão funcionando corretamente!" "SUCCESS"
        Show-ServicesInfo
    } else {
        Write-Log "⚠ Alguns serviços podem não estar funcionando corretamente" "WARN"
        Write-Log "Verifique os logs com: docker-compose logs <service-name>" "INFO"
        Show-ServicesInfo
    }
    
    Write-Log "Setup concluído!" "SUCCESS"
}

# Executar função principal
Main

# Instruções finais
Write-Host ""
Write-Host "=== Comandos Úteis ===" -ForegroundColor Magenta
Write-Host "Para parar os serviços: .\scripts\setup-environment.ps1 -StopServices" -ForegroundColor White
Write-Host "Para limpeza completa: .\scripts\setup-environment.ps1 -CleanUp" -ForegroundColor White
Write-Host "Para ver logs: docker-compose logs -f <service-name>" -ForegroundColor White
Write-Host "Para ver status: docker-compose ps" -ForegroundColor White
Write-Host ""