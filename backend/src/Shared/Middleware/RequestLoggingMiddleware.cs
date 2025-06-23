using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reembolso.Shared.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace Reembolso.Shared.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly ILoggingService _loggingService;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, ILoggingService loggingService)
        {
            _next = next;
            _logger = logger;
            _loggingService = loggingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Gerar ID único para a requisição
            var requestId = Guid.NewGuid().ToString();
            context.Items["RequestId"] = requestId;

            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                // Capturar informações da requisição
                var requestInfo = await CaptureRequestInfoAsync(context, requestId);

                // Capturar resposta
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Executar próximo middleware
                await _next(context);

                stopwatch.Stop();

                // Capturar informações da resposta
                var responseInfo = await CaptureResponseInfoAsync(context, responseBody, originalBodyStream);

                // Registrar log da requisição
                await LogRequestAsync(requestInfo, responseInfo, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Registrar erro
                _loggingService.LogError(ex, "Erro durante processamento da requisição", 
                    GetUserId(context), new { RequestId = requestId });

                // Registrar requisição com erro
                _loggingService.LogApiRequest(
                    context.Request.Method,
                    context.Request.Path,
                    500,
                    stopwatch.Elapsed,
                    GetUserId(context),
                    GetIpAddress(context),
                    context.Request.Headers["User-Agent"],
                    null,
                    ex.Message
                );

                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<RequestInfo> CaptureRequestInfoAsync(HttpContext context, string requestId)
        {
            var request = context.Request;
            var requestInfo = new RequestInfo
            {
                RequestId = requestId,
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                IpAddress = GetIpAddress(context),
                UserAgent = request.Headers["User-Agent"],
                UserId = GetUserId(context),
                Timestamp = DateTime.UtcNow
            };

            // Capturar corpo da requisição se necessário
            if (ShouldLogRequestBody(request))
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                requestInfo.Body = Encoding.UTF8.GetString(buffer);
                request.Body.Position = 0;
            }

            return requestInfo;
        }

        private async Task<ResponseInfo> CaptureResponseInfoAsync(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
        {
            var response = context.Response;
            var responseInfo = new ResponseInfo
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                ContentType = response.ContentType,
                ContentLength = responseBody.Length
            };

            // Capturar corpo da resposta se necessário
            if (ShouldLogResponseBody(response))
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                responseInfo.Body = await new StreamReader(responseBody).ReadToEndAsync();
            }

            // Copiar resposta para o stream original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            return responseInfo;
        }

        private async Task LogRequestAsync(RequestInfo requestInfo, ResponseInfo responseInfo, TimeSpan duration)
        {
            try
            {
                // Log básico da API
                _loggingService.LogApiRequest(
                    requestInfo.Method,
                    requestInfo.Path,
                    responseInfo.StatusCode,
                    duration,
                    requestInfo.UserId,
                    requestInfo.IpAddress,
                    requestInfo.UserAgent,
                    ShouldLogRequestBody(requestInfo) ? requestInfo.Body : null,
                    ShouldLogResponseBody(responseInfo) ? responseInfo.Body : null
                );

                // Log detalhado se necessário
                if (ShouldLogDetailed(requestInfo, responseInfo))
                {
                    var detailedLog = new
                    {
                        Request = new
                        {
                            requestInfo.RequestId,
                            requestInfo.Method,
                            requestInfo.Path,
                            requestInfo.QueryString,
                            Headers = FilterSensitiveHeaders(requestInfo.Headers),
                            requestInfo.IpAddress,
                            requestInfo.UserAgent,
                            requestInfo.UserId,
                            Body = ShouldLogRequestBody(requestInfo) ? FilterSensitiveData(requestInfo.Body) : null
                        },
                        Response = new
                        {
                            responseInfo.StatusCode,
                            Headers = FilterSensitiveHeaders(responseInfo.Headers),
                            responseInfo.ContentType,
                            responseInfo.ContentLength,
                            Body = ShouldLogResponseBody(responseInfo) ? FilterSensitiveData(responseInfo.Body) : null
                        },
                        Performance = new
                        {
                            Duration = duration.TotalMilliseconds,
                            Timestamp = requestInfo.Timestamp
                        }
                    };

                    _loggingService.LogInformation("Requisição HTTP detalhada", "HTTP", detailedLog);
                }

                // Log de performance se demorou muito
                if (duration.TotalSeconds > 5)
                {
                    _loggingService.LogPerformance(
                        $"{requestInfo.Method} {requestInfo.Path}",
                        duration,
                        responseInfo.StatusCode < 400
                    );
                }

                // Log de segurança para tentativas suspeitas
                if (IsSuspiciousRequest(requestInfo, responseInfo))
                {
                    _loggingService.LogSecurityEvent(
                        "SuspiciousRequest",
                        $"Requisição suspeita detectada: {requestInfo.Method} {requestInfo.Path}",
                        requestInfo.UserId,
                        requestInfo.IpAddress,
                        requestInfo.UserAgent,
                        new { requestInfo.RequestId, responseInfo.StatusCode }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar log da requisição {RequestId}", requestInfo.RequestId);
            }
        }

        private bool ShouldLogRequestBody(HttpRequest request)
        {
            return ShouldLogRequestBody(new RequestInfo
            {
                Method = request.Method,
                Path = request.Path,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            });
        }

        private bool ShouldLogRequestBody(RequestInfo requestInfo)
        {
            // Não logar corpo para métodos GET e DELETE
            if (requestInfo.Method == "GET" || requestInfo.Method == "DELETE")
                return false;

            // Não logar uploads de arquivos
            if (requestInfo.Headers.ContainsKey("Content-Type") && 
                requestInfo.Headers["Content-Type"].Contains("multipart/form-data"))
                return false;

            // Não logar endpoints sensíveis
            var sensitivePaths = new[] { "/auth/login", "/auth/register", "/users/password" };
            if (sensitivePaths.Any(path => requestInfo.Path.Contains(path)))
                return false;

            return true;
        }

        private bool ShouldLogResponseBody(HttpResponse response)
        {
            return ShouldLogResponseBody(new ResponseInfo
            {
                StatusCode = response.StatusCode,
                ContentType = response.ContentType,
                ContentLength = response.Body.Length
            });
        }

        private bool ShouldLogResponseBody(ResponseInfo responseInfo)
        {
            // Não logar respostas muito grandes
            if (responseInfo.ContentLength > 1024 * 1024) // 1MB
                return false;

            // Não logar arquivos binários
            if (!string.IsNullOrEmpty(responseInfo.ContentType))
            {
                var binaryTypes = new[] { "image/", "video/", "audio/", "application/pdf", "application/zip" };
                if (binaryTypes.Any(type => responseInfo.ContentType.StartsWith(type)))
                    return false;
            }

            // Logar apenas erros e algumas respostas de sucesso
            return responseInfo.StatusCode >= 400 || responseInfo.StatusCode == 201;
        }

        private bool ShouldLogDetailed(RequestInfo requestInfo, ResponseInfo responseInfo)
        {
            // Log detalhado para erros
            if (responseInfo.StatusCode >= 400)
                return true;

            // Log detalhado para operações importantes
            var importantPaths = new[] { "/auth/", "/users/", "/solicitacoes/", "/relatorios/" };
            if (importantPaths.Any(path => requestInfo.Path.Contains(path)))
                return true;

            return false;
        }

        private bool IsSuspiciousRequest(RequestInfo requestInfo, ResponseInfo responseInfo)
        {
            // Múltiplas tentativas de login falhadas
            if (requestInfo.Path.Contains("/auth/login") && responseInfo.StatusCode == 401)
                return true;

            // Tentativas de acesso a recursos não autorizados
            if (responseInfo.StatusCode == 403)
                return true;

            // Tentativas de SQL injection ou XSS
            var suspiciousPatterns = new[] { "<script", "javascript:", "SELECT", "INSERT", "DELETE", "DROP" };
            if (suspiciousPatterns.Any(pattern => 
                requestInfo.QueryString?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true ||
                requestInfo.Body?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true))
                return true;

            return false;
        }

        private Dictionary<string, string> FilterSensitiveHeaders(Dictionary<string, string> headers)
        {
            var sensitiveHeaders = new[] { "Authorization", "Cookie", "X-API-Key" };
            var filtered = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                if (sensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                {
                    filtered[header.Key] = "[FILTERED]";
                }
                else
                {
                    filtered[header.Key] = header.Value;
                }
            }

            return filtered;
        }

        private string FilterSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            try
            {
                // Tentar parsear como JSON e filtrar campos sensíveis
                var jsonDoc = JsonDocument.Parse(data);
                var filtered = FilterJsonElement(jsonDoc.RootElement);
                return JsonSerializer.Serialize(filtered);
            }
            catch
            {
                // Se não for JSON válido, retornar como está
                return data;
            }
        }

        private object FilterJsonElement(JsonElement element)
        {
            var sensitiveFields = new[] { "password", "token", "secret", "key", "authorization" };

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        if (sensitiveFields.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            obj[property.Name] = "[FILTERED]";
                        }
                        else
                        {
                            obj[property.Name] = FilterJsonElement(property.Value);
                        }
                    }
                    return obj;

                case JsonValueKind.Array:
                    return element.EnumerateArray().Select(FilterJsonElement).ToArray();

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    return element.GetDecimal();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                case JsonValueKind.Null:
                    return null;

                default:
                    return element.ToString();
            }
        }

        private int? GetUserId(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string GetIpAddress(HttpContext context)
        {
            // Verificar headers de proxy primeiro
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

    // Classes auxiliares
    public class RequestInfo
    {
        public string RequestId { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public int? UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ResponseInfo
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
    }
}