using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reembolso.Shared.DTOs;
using System.Net;
using System.Text.Json;

namespace Reembolso.Shared.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ArgumentException => new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = "Dados inválidos",
                Erros = new List<string> { exception.Message }
            },
            UnauthorizedAccessException => new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = "Acesso não autorizado",
                Erros = new List<string> { exception.Message }
            },
            KeyNotFoundException => new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = "Recurso não encontrado",
                Erros = new List<string> { exception.Message }
            },
            _ => new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = "Erro interno do servidor",
                Erros = new List<string> { "Ocorreu um erro inesperado" }
            }
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}