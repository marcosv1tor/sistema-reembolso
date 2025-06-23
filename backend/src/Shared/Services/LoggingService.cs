using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Reembolso.Shared.DTOs;
using Reembolso.Shared.Interfaces;

namespace Reembolso.Shared.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _serilogLogger;

        public LoggingService(ILogger<LoggingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _serilogLogger = ConfigureSerilog();
        }

        private Serilog.ILogger ConfigureSerilog()
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "ReembolsoSystem")
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development")
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("ProcessId", Environment.ProcessId)
                .WriteTo.Console(outputTemplate: 
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/reembolso-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

            // Configurar Elasticsearch se habilitado
            var elasticsearchEnabled = _configuration.GetValue<bool>("Logging:Elasticsearch:Enabled");
            if (elasticsearchEnabled)
            {
                var elasticsearchUrl = _configuration["Logging:Elasticsearch:Url"] ?? "http://localhost:9200";
                var indexName = _configuration["Logging:Elasticsearch:IndexName"] ?? "reembolso-logs";
                
                loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
                {
                    IndexFormat = $"{indexName}-{{0:yyyy.MM.dd}}",
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    TemplateName = "reembolso-template",
                    NumberOfShards = 2,
                    NumberOfReplicas = 1,
                    BufferBaseFilename = "logs/elasticsearch-buffer",
                    BufferLogShippingInterval = TimeSpan.FromSeconds(5)
                });
            }

            return loggerConfig.CreateLogger();
        }

        public async Task LogAsync(LogLevel level, string message, object data = null, Exception exception = null, 
            string userId = null, string correlationId = null, string microservice = null)
        {
            try
            {
                var logEntry = new LogEntryDto
                {
                    Timestamp = DateTime.UtcNow,
                    Level = level.ToString(),
                    Message = message,
                    Data = data,
                    Exception = exception?.ToString(),
                    UserId = userId,
                    CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
                    Microservice = microservice ?? GetMicroserviceName(),
                    MachineName = Environment.MachineName,
                    ProcessId = Environment.ProcessId
                };

                await Task.Run(() =>
                {
                    var serilogLevel = ConvertToSerilogLevel(level);
                    
                    _serilogLogger
                        .ForContext("UserId", userId)
                        .ForContext("CorrelationId", logEntry.CorrelationId)
                        .ForContext("Microservice", logEntry.Microservice)
                        .ForContext("Data", data, true)
                        .Write(serilogLevel, exception, message);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar log: {Message}", message);
            }
        }

        public async Task LogInformationAsync(string message, object data = null, string userId = null, 
            string correlationId = null, string microservice = null)
        {
            await LogAsync(LogLevel.Information, message, data, null, userId, correlationId, microservice);
        }

        public async Task LogWarningAsync(string message, object data = null, string userId = null, 
            string correlationId = null, string microservice = null)
        {
            await LogAsync(LogLevel.Warning, message, data, null, userId, correlationId, microservice);
        }

        public async Task LogErrorAsync(string message, Exception exception = null, object data = null, 
            string userId = null, string correlationId = null, string microservice = null)
        {
            await LogAsync(LogLevel.Error, message, data, exception, userId, correlationId, microservice);
        }

        public async Task LogCriticalAsync(string message, Exception exception = null, object data = null, 
            string userId = null, string correlationId = null, string microservice = null)
        {
            await LogAsync(LogLevel.Critical, message, data, exception, userId, correlationId, microservice);
        }

        public async Task LogDebugAsync(string message, object data = null, string userId = null, 
            string correlationId = null, string microservice = null)
        {
            await LogAsync(LogLevel.Debug, message, data, null, userId, correlationId, microservice);
        }

        public async Task LogUserActionAsync(string action, string userId, object data = null, 
            string correlationId = null, string microservice = null)
        {
            var message = $"Ação do usuário: {action}";
            var logData = new
            {
                Action = action,
                UserId = userId,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await LogInformationAsync(message, logData, userId, correlationId, microservice);
        }

        public async Task LogApiRequestAsync(string method, string endpoint, string userId = null, 
            object requestData = null, string correlationId = null, string microservice = null)
        {
            var message = $"API Request: {method} {endpoint}";
            var logData = new
            {
                Method = method,
                Endpoint = endpoint,
                RequestData = requestData,
                Timestamp = DateTime.UtcNow
            };

            await LogInformationAsync(message, logData, userId, correlationId, microservice);
        }

        public async Task LogApiResponseAsync(string method, string endpoint, int statusCode, 
            TimeSpan duration, string userId = null, object responseData = null, 
            string correlationId = null, string microservice = null)
        {
            var message = $"API Response: {method} {endpoint} - {statusCode} ({duration.TotalMilliseconds}ms)";
            var logData = new
            {
                Method = method,
                Endpoint = endpoint,
                StatusCode = statusCode,
                Duration = duration.TotalMilliseconds,
                ResponseData = responseData,
                Timestamp = DateTime.UtcNow
            };

            var level = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            await LogAsync(level, message, logData, null, userId, correlationId, microservice);
        }

        public async Task LogBusinessEventAsync(string eventName, object eventData, string userId = null, 
            string correlationId = null, string microservice = null)
        {
            var message = $"Evento de negócio: {eventName}";
            var logData = new
            {
                EventName = eventName,
                EventData = eventData,
                Timestamp = DateTime.UtcNow
            };

            await LogInformationAsync(message, logData, userId, correlationId, microservice);
        }

        public async Task LogSecurityEventAsync(string eventType, string description, string userId = null, 
            object additionalData = null, string correlationId = null, string microservice = null)
        {
            var message = $"Evento de segurança: {eventType} - {description}";
            var logData = new
            {
                EventType = eventType,
                Description = description,
                AdditionalData = additionalData,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            await LogWarningAsync(message, logData, userId, correlationId, microservice);
        }

        public async Task LogPerformanceAsync(string operation, TimeSpan duration, 
            object additionalData = null, string userId = null, string correlationId = null, 
            string microservice = null)
        {
            var message = $"Performance: {operation} executado em {duration.TotalMilliseconds}ms";
            var logData = new
            {
                Operation = operation,
                Duration = duration.TotalMilliseconds,
                AdditionalData = additionalData,
                Timestamp = DateTime.UtcNow
            };

            var level = duration.TotalSeconds > 5 ? LogLevel.Warning : LogLevel.Information;
            await LogAsync(level, message, logData, null, userId, correlationId, microservice);
        }

        public async Task LogDatabaseOperationAsync(string operation, string tableName, 
            TimeSpan duration, int recordsAffected = 0, string userId = null, 
            string correlationId = null, string microservice = null)
        {
            var message = $"Operação BD: {operation} em {tableName} - {recordsAffected} registros ({duration.TotalMilliseconds}ms)";
            var logData = new
            {
                Operation = operation,
                TableName = tableName,
                Duration = duration.TotalMilliseconds,
                RecordsAffected = recordsAffected,
                Timestamp = DateTime.UtcNow
            };

            var level = duration.TotalSeconds > 2 ? LogLevel.Warning : LogLevel.Information;
            await LogAsync(level, message, logData, null, userId, correlationId, microservice);
        }

        public async Task LogExternalServiceCallAsync(string serviceName, string operation, 
            TimeSpan duration, bool success, string errorMessage = null, string userId = null, 
            string correlationId = null, string microservice = null)
        {
            var status = success ? "Sucesso" : "Falha";
            var message = $"Chamada externa: {serviceName}.{operation} - {status} ({duration.TotalMilliseconds}ms)";
            var logData = new
            {
                ServiceName = serviceName,
                Operation = operation,
                Duration = duration.TotalMilliseconds,
                Success = success,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            };

            var level = success ? LogLevel.Information : LogLevel.Error;
            await LogAsync(level, message, logData, null, userId, correlationId, microservice);
        }

        public string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task<List<LogEntryDto>> SearchLogsAsync(LogSearchCriteriaDto criteria)
        {
            // Esta implementação seria mais complexa em um cenário real,
            // provavelmente consultando Elasticsearch ou outro sistema de logs
            // Por enquanto, retornamos uma lista vazia
            await Task.CompletedTask;
            return new List<LogEntryDto>();
        }

        private LogEventLevel ConvertToSerilogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }

        private string GetMicroserviceName()
        {
            return _configuration["Microservice:Name"] ?? 
                   Environment.GetEnvironmentVariable("MICROSERVICE_NAME") ?? 
                   "Unknown";
        }

        private string GetClientIpAddress()
        {
            // Em um cenário real, isso seria obtido do HttpContext
            return "Unknown";
        }

        private string GetUserAgent()
        {
            // Em um cenário real, isso seria obtido do HttpContext
            return "Unknown";
        }

        public void Dispose()
        {
            _serilogLogger?.Dispose();
        }
    }

    // Middleware para capturar informações de request/response
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;

        public LoggingMiddleware(RequestDelegate next, ILoggingService loggingService)
        {
            _next = next;
            _loggingService = loggingService;
        }

        public async Task InvokeAsync(Microsoft.AspNetCore.Http.HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? _loggingService.GenerateCorrelationId();
            
            context.Response.Headers.Add("X-Correlation-ID", correlationId);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                await _loggingService.LogApiRequestAsync(
                    context.Request.Method,
                    context.Request.Path,
                    context.User?.Identity?.Name,
                    null, // Request data seria capturado aqui se necessário
                    correlationId
                );

                await _next(context);

                stopwatch.Stop();

                await _loggingService.LogApiResponseAsync(
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.Elapsed,
                    context.User?.Identity?.Name,
                    null, // Response data seria capturado aqui se necessário
                    correlationId
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _loggingService.LogErrorAsync(
                    $"Erro não tratado em {context.Request.Method} {context.Request.Path}",
                    ex,
                    new { 
                        Method = context.Request.Method,
                        Path = context.Request.Path,
                        Duration = stopwatch.Elapsed.TotalMilliseconds
                    },
                    context.User?.Identity?.Name,
                    correlationId
                );

                throw;
            }
        }
    }
}