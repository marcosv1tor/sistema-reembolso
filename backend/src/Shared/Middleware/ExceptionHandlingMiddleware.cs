using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reembolso.Shared.Interfaces;
using Reembolso.Shared.DTOs;
using System.Security.Claims;

namespace Reembolso.Shared.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly ILoggingService _loggingService;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, ILoggingService loggingService)
        {
            _next = next;
            _logger = logger;
            _loggingService = loggingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var requestId = context.Items["RequestId"]?.ToString() ?? Guid.NewGuid().ToString();
            var userId = GetUserId(context);
            var ipAddress = GetIpAddress(context);
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // Registrar erro no sistema de logging
            _loggingService.LogError(exception, "Exceção não tratada", userId, new
            {
                RequestId = requestId,
                Path = context.Request.Path,
                Method = context.Request.Method,
                QueryString = context.Request.QueryString.ToString(),
                IpAddress = ipAddress,
                UserAgent = userAgent
            });

            // Determinar tipo de erro e resposta apropriada
            var errorResponse = CreateErrorResponse(exception, requestId);

            // Configurar resposta HTTP
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.StatusCode;

            // Registrar evento de segurança se necessário
            if (IsSecurityException(exception))
            {
                _loggingService.LogSecurityEvent(
                    "SecurityException",
                    $"Exceção de segurança: {exception.GetType().Name}",
                    userId,
                    ipAddress,
                    userAgent,
                    new { RequestId = requestId, ExceptionType = exception.GetType().Name }
                );
            }

            // Serializar e enviar resposta
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponseDto CreateErrorResponse(Exception exception, string requestId)
        {
            var response = new ErrorResponseDto
            {
                RequestId = requestId,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = "Validation Error";
                    response.Message = "Os dados fornecidos são inválidos";
                    response.Details = validationEx.Errors;
                    break;

                case UnauthorizedException unauthorizedEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Error = "Unauthorized";
                    response.Message = "Acesso não autorizado";
                    response.Details = new { Reason = unauthorizedEx.Message };
                    break;

                case ForbiddenException forbiddenEx:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Error = "Forbidden";
                    response.Message = "Acesso negado";
                    response.Details = new { Reason = forbiddenEx.Message };
                    break;

                case NotFoundException notFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Error = "Not Found";
                    response.Message = "Recurso não encontrado";
                    response.Details = new { Resource = notFoundEx.Resource, Id = notFoundEx.Id };
                    break;

                case ConflictException conflictEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Error = "Conflict";
                    response.Message = "Conflito de dados";
                    response.Details = new { Reason = conflictEx.Message };
                    break;

                case BusinessRuleException businessEx:
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    response.Error = "Business Rule Violation";
                    response.Message = "Regra de negócio violada";
                    response.Details = new { Rule = businessEx.Rule, Reason = businessEx.Message };
                    break;

                case ExternalServiceException externalEx:
                    response.StatusCode = (int)HttpStatusCode.BadGateway;
                    response.Error = "External Service Error";
                    response.Message = "Erro em serviço externo";
                    response.Details = new { Service = externalEx.ServiceName, Reason = externalEx.Message };
                    break;

                case TimeoutException timeoutEx:
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Error = "Timeout";
                    response.Message = "Operação expirou";
                    response.Details = new { Reason = timeoutEx.Message };
                    break;

                case ArgumentException argumentEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = "Invalid Argument";
                    response.Message = "Argumento inválido";
                    response.Details = new { Parameter = argumentEx.ParamName, Reason = argumentEx.Message };
                    break;

                case InvalidOperationException invalidOpEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = "Invalid Operation";
                    response.Message = "Operação inválida";
                    response.Details = new { Reason = invalidOpEx.Message };
                    break;

                case NotSupportedException notSupportedEx:
                    response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    response.Error = "Not Supported";
                    response.Message = "Operação não suportada";
                    response.Details = new { Reason = notSupportedEx.Message };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Error = "Internal Server Error";
                    response.Message = "Erro interno do servidor";
                    response.Details = new { Type = exception.GetType().Name };
                    break;
            }

            return response;
        }

        private bool IsSecurityException(Exception exception)
        {
            return exception is UnauthorizedException ||
                   exception is ForbiddenException ||
                   exception.GetType().Name.Contains("Security") ||
                   exception.GetType().Name.Contains("Authentication") ||
                   exception.GetType().Name.Contains("Authorization");
        }

        private int? GetUserId(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string GetIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    // DTOs para resposta de erro
    public class ErrorResponseDto
    {
        public int StatusCode { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public object Details { get; set; }
        public string RequestId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // Exceções customizadas
    public class ValidationException : Exception
    {
        public object Errors { get; }

        public ValidationException(string message, object errors = null) : base(message)
        {
            Errors = errors;
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Acesso não autorizado") : base(message) { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Acesso negado") : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public string Resource { get; }
        public object Id { get; }

        public NotFoundException(string resource, object id) : base($"{resource} com ID {id} não encontrado")
        {
            Resource = resource;
            Id = id;
        }

        public NotFoundException(string message) : base(message) { }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    public class BusinessRuleException : Exception
    {
        public string Rule { get; }

        public BusinessRuleException(string rule, string message) : base(message)
        {
            Rule = rule;
        }
    }

    public class ExternalServiceException : Exception
    {
        public string ServiceName { get; }

        public ExternalServiceException(string serviceName, string message) : base(message)
        {
            ServiceName = serviceName;
        }

        public ExternalServiceException(string serviceName, string message, Exception innerException) : base(message, innerException)
        {
            ServiceName = serviceName;
        }
    }
}