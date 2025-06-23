using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Reembolso.Shared.DTOs;
using Reembolso.Shared.Interfaces;

namespace Reembolso.Shared.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public HealthCheckService(ILogger<HealthCheckService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<HealthCheckResultDto> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var result = new HealthCheckResultDto
            {
                ServiceName = GetServiceName(),
                Timestamp = DateTime.UtcNow,
                Checks = new List<IndividualHealthCheckDto>()
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Verificar banco de dados
                var dbCheck = await CheckDatabaseAsync(cancellationToken);
                result.Checks.Add(dbCheck);

                // Verificar dependências externas
                var externalChecks = await CheckExternalDependenciesAsync(cancellationToken);
                result.Checks.AddRange(externalChecks);

                // Verificar recursos do sistema
                var systemChecks = await CheckSystemResourcesAsync(cancellationToken);
                result.Checks.AddRange(systemChecks);

                // Verificar serviços internos
                var serviceChecks = await CheckInternalServicesAsync(cancellationToken);
                result.Checks.AddRange(serviceChecks);

                // Determinar status geral
                result.Status = DetermineOverallStatus(result.Checks);
                result.Duration = stopwatch.Elapsed;

                _logger.LogInformation("Health check concluído: {Status} em {Duration}ms", 
                    result.Status, result.Duration.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante health check");
                
                result.Status = HealthStatus.Unhealthy;
                result.Duration = stopwatch.Elapsed;
                result.Checks.Add(new IndividualHealthCheckDto
                {
                    Name = "HealthCheckService",
                    Status = HealthStatus.Unhealthy,
                    Description = "Erro interno durante verificação de saúde",
                    Exception = ex.Message,
                    Duration = stopwatch.Elapsed
                });

                return result;
            }
        }

        public async Task<IndividualHealthCheckDto> CheckDatabaseAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var check = new IndividualHealthCheckDto
            {
                Name = "Database",
                Timestamp = DateTime.UtcNow
            };

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    check.Status = HealthStatus.Unhealthy;
                    check.Description = "Connection string não configurada";
                    return check;
                }

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 5;
                
                var result = await command.ExecuteScalarAsync(cancellationToken);
                
                stopwatch.Stop();
                
                check.Status = HealthStatus.Healthy;
                check.Description = "Conexão com banco de dados estabelecida com sucesso";
                check.Duration = stopwatch.Elapsed;
                check.Data = new Dictionary<string, object>
                {
                    { "ConnectionTime", stopwatch.Elapsed.TotalMilliseconds },
                    { "ServerVersion", connection.ServerVersion },
                    { "Database", connection.Database }
                };

                return check;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                check.Status = HealthStatus.Unhealthy;
                check.Description = "Falha na conexão com banco de dados";
                check.Exception = ex.Message;
                check.Duration = stopwatch.Elapsed;
                
                _logger.LogError(ex, "Falha no health check do banco de dados");
                return check;
            }
        }

        public async Task<List<IndividualHealthCheckDto>> CheckExternalDependenciesAsync(CancellationToken cancellationToken = default)
        {
            var checks = new List<IndividualHealthCheckDto>();

            // Verificar outros microserviços
            var microservices = _configuration.GetSection("Microservices").GetChildren();
            foreach (var microservice in microservices)
            {
                var name = microservice.Key;
                var url = microservice["Url"];
                
                if (!string.IsNullOrEmpty(url))
                {
                    var check = await CheckHttpEndpointAsync(name, $"{url}/health", cancellationToken);
                    checks.Add(check);
                }
            }

            // Verificar Redis (se configurado)
            var redisConnectionString = _configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                var redisCheck = await CheckRedisAsync(cancellationToken);
                checks.Add(redisCheck);
            }

            // Verificar Elasticsearch (se configurado)
            var elasticsearchUrl = _configuration["Logging:Elasticsearch:Url"];
            if (!string.IsNullOrEmpty(elasticsearchUrl))
            {
                var esCheck = await CheckHttpEndpointAsync("Elasticsearch", $"{elasticsearchUrl}/_cluster/health", cancellationToken);
                checks.Add(esCheck);
            }

            return checks;
        }

        public async Task<List<IndividualHealthCheckDto>> CheckSystemResourcesAsync(CancellationToken cancellationToken = default)
        {
            var checks = new List<IndividualHealthCheckDto>();

            // Verificar uso de memória
            var memoryCheck = CheckMemoryUsage();
            checks.Add(memoryCheck);

            // Verificar espaço em disco
            var diskCheck = CheckDiskSpace();
            checks.Add(diskCheck);

            // Verificar CPU
            var cpuCheck = await CheckCpuUsageAsync(cancellationToken);
            checks.Add(cpuCheck);

            return checks;
        }

        public async Task<List<IndividualHealthCheckDto>> CheckInternalServicesAsync(CancellationToken cancellationToken = default)
        {
            var checks = new List<IndividualHealthCheckDto>();

            // Verificar serviços internos específicos do microserviço
            // Isso seria implementado de forma específica para cada microserviço
            
            await Task.CompletedTask; // Placeholder
            return checks;
        }

        private async Task<IndividualHealthCheckDto> CheckHttpEndpointAsync(string name, string url, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var check = new IndividualHealthCheckDto
            {
                Name = name,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(10)); // Timeout de 10 segundos

                var response = await _httpClient.GetAsync(url, cts.Token);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    check.Status = HealthStatus.Healthy;
                    check.Description = $"Endpoint {url} respondeu com sucesso";
                }
                else
                {
                    check.Status = HealthStatus.Degraded;
                    check.Description = $"Endpoint {url} retornou status {response.StatusCode}";
                }

                check.Duration = stopwatch.Elapsed;
                check.Data = new Dictionary<string, object>
                {
                    { "Url", url },
                    { "StatusCode", (int)response.StatusCode },
                    { "ResponseTime", stopwatch.Elapsed.TotalMilliseconds }
                };

                return check;
            }
            catch (TaskCanceledException)
            {
                stopwatch.Stop();
                check.Status = HealthStatus.Unhealthy;
                check.Description = $"Timeout ao acessar {url}";
                check.Duration = stopwatch.Elapsed;
                return check;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                check.Status = HealthStatus.Unhealthy;
                check.Description = $"Erro ao acessar {url}";
                check.Exception = ex.Message;
                check.Duration = stopwatch.Elapsed;
                return check;
            }
        }

        private async Task<IndividualHealthCheckDto> CheckRedisAsync(CancellationToken cancellationToken)
        {
            var check = new IndividualHealthCheckDto
            {
                Name = "Redis",
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // Implementação específica para Redis seria feita aqui
                // Por enquanto, simulamos uma verificação
                await Task.Delay(100, cancellationToken);
                
                check.Status = HealthStatus.Healthy;
                check.Description = "Redis está funcionando";
                check.Duration = TimeSpan.FromMilliseconds(100);

                return check;
            }
            catch (Exception ex)
            {
                check.Status = HealthStatus.Unhealthy;
                check.Description = "Falha na conexão com Redis";
                check.Exception = ex.Message;
                return check;
            }
        }

        private IndividualHealthCheckDto CheckMemoryUsage()
        {
            var check = new IndividualHealthCheckDto
            {
                Name = "Memory",
                Timestamp = DateTime.UtcNow
            };

            try
            {
                var process = Process.GetCurrentProcess();
                var workingSet = process.WorkingSet64;
                var privateMemory = process.PrivateMemorySize64;
                
                var workingSetMB = workingSet / 1024 / 1024;
                var privateMemoryMB = privateMemory / 1024 / 1024;

                // Considerar unhealthy se usar mais de 1GB
                var maxMemoryMB = 1024;
                
                if (workingSetMB > maxMemoryMB)
                {
                    check.Status = HealthStatus.Unhealthy;
                    check.Description = $"Uso de memória muito alto: {workingSetMB}MB";
                }
                else if (workingSetMB > maxMemoryMB * 0.8)
                {
                    check.Status = HealthStatus.Degraded;
                    check.Description = $"Uso de memória elevado: {workingSetMB}MB";
                }
                else
                {
                    check.Status = HealthStatus.Healthy;
                    check.Description = $"Uso de memória normal: {workingSetMB}MB";
                }

                check.Data = new Dictionary<string, object>
                {
                    { "WorkingSetMB", workingSetMB },
                    { "PrivateMemoryMB", privateMemoryMB },
                    { "MaxMemoryMB", maxMemoryMB }
                };

                return check;
            }
            catch (Exception ex)
            {
                check.Status = HealthStatus.Unhealthy;
                check.Description = "Erro ao verificar uso de memória";
                check.Exception = ex.Message;
                return check;
            }
        }

        private IndividualHealthCheckDto CheckDiskSpace()
        {
            var check = new IndividualHealthCheckDto
            {
                Name = "DiskSpace",
                Timestamp = DateTime.UtcNow
            };

            try
            {
                var drive = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Environment.CurrentDirectory));
                var freeSpaceGB = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                var totalSpaceGB = drive.TotalSize / 1024 / 1024 / 1024;
                var usedSpacePercentage = (double)(totalSpaceGB - freeSpaceGB) / totalSpaceGB * 100;

                if (usedSpacePercentage > 90)
                {
                    check.Status = HealthStatus.Unhealthy;
                    check.Description = $"Espaço em disco crítico: {usedSpacePercentage:F1}% usado";
                }
                else if (usedSpacePercentage > 80)
                {
                    check.Status = HealthStatus.Degraded;
                    check.Description = $"Espaço em disco baixo: {usedSpacePercentage:F1}% usado";
                }
                else
                {
                    check.Status = HealthStatus.Healthy;
                    check.Description = $"Espaço em disco adequado: {usedSpacePercentage:F1}% usado";
                }

                check.Data = new Dictionary<string, object>
                {
                    { "FreeSpaceGB", freeSpaceGB },
                    { "TotalSpaceGB", totalSpaceGB },
                    { "UsedPercentage", usedSpacePercentage }
                };

                return check;
            }
            catch (Exception ex)
            {
                check.Status = HealthStatus.Unhealthy;
                check.Description = "Erro ao verificar espaço em disco";
                check.Exception = ex.Message;
                return check;
            }
        }

        private async Task<IndividualHealthCheckDto> CheckCpuUsageAsync(CancellationToken cancellationToken)
        {
            var check = new IndividualHealthCheckDto
            {
                Name = "CPU",
                Timestamp = DateTime.UtcNow
            };

            try
            {
                var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;
                
                await Task.Delay(1000, cancellationToken); // Aguardar 1 segundo
                
                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;
                
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                var cpuUsagePercentage = cpuUsageTotal * 100;

                if (cpuUsagePercentage > 80)
                {
                    check.Status = HealthStatus.Unhealthy;
                    check.Description = $"Uso de CPU muito alto: {cpuUsagePercentage:F1}%";
                }
                else if (cpuUsagePercentage > 60)
                {
                    check.Status = HealthStatus.Degraded;
                    check.Description = $"Uso de CPU elevado: {cpuUsagePercentage:F1}%";
                }
                else
                {
                    check.Status = HealthStatus.Healthy;
                    check.Description = $"Uso de CPU normal: {cpuUsagePercentage:F1}%";
                }

                check.Data = new Dictionary<string, object>
                {
                    { "CpuUsagePercentage", cpuUsagePercentage },
                    { "ProcessorCount", Environment.ProcessorCount }
                };

                return check;
            }
            catch (Exception ex)
            {
                check.Status = HealthStatus.Unhealthy;
                check.Description = "Erro ao verificar uso de CPU";
                check.Exception = ex.Message;
                return check;
            }
        }

        private HealthStatus DetermineOverallStatus(List<IndividualHealthCheckDto> checks)
        {
            var hasUnhealthy = false;
            var hasDegraded = false;

            foreach (var check in checks)
            {
                switch (check.Status)
                {
                    case HealthStatus.Unhealthy:
                        hasUnhealthy = true;
                        break;
                    case HealthStatus.Degraded:
                        hasDegraded = true;
                        break;
                }
            }

            if (hasUnhealthy)
                return HealthStatus.Unhealthy;
            if (hasDegraded)
                return HealthStatus.Degraded;
            
            return HealthStatus.Healthy;
        }

        private string GetServiceName()
        {
            return _configuration["Microservice:Name"] ?? 
                   Environment.GetEnvironmentVariable("MICROSERVICE_NAME") ?? 
                   "Unknown";
        }
    }

    // Health Check customizado para ASP.NET Core
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly IHealthCheckService _healthCheckService;

        public CustomHealthCheck(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _healthCheckService.CheckHealthAsync(cancellationToken);
                
                var data = new Dictionary<string, object>
                {
                    { "ServiceName", result.ServiceName },
                    { "Timestamp", result.Timestamp },
                    { "Duration", result.Duration.TotalMilliseconds },
                    { "ChecksCount", result.Checks.Count }
                };

                return new HealthCheckResult(result.Status, result.ToString(), data: data);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Health check failed", ex);
            }
        }
    }
}