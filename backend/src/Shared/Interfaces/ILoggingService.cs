using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reembolso.Shared.DTOs;

namespace Reembolso.Shared.Interfaces
{
    public interface ILoggingService
    {
        /// <summary>
        /// Registra uma ação do usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="action">Ação realizada</param>
        /// <param name="details">Detalhes adicionais</param>
        /// <param name="ipAddress">Endereço IP</param>
        /// <param name="userAgent">User Agent</param>
        void LogUserAction(int userId, string action, object details = null, string ipAddress = null, string userAgent = null);

        /// <summary>
        /// Registra uma requisição da API
        /// </summary>
        /// <param name="method">Método HTTP</param>
        /// <param name="path">Caminho da requisição</param>
        /// <param name="statusCode">Código de status da resposta</param>
        /// <param name="duration">Duração da requisição</param>
        /// <param name="userId">ID do usuário (se autenticado)</param>
        /// <param name="ipAddress">Endereço IP</param>
        /// <param name="userAgent">User Agent</param>
        /// <param name="requestBody">Corpo da requisição</param>
        /// <param name="responseBody">Corpo da resposta</param>
        void LogApiRequest(string method, string path, int statusCode, TimeSpan duration, int? userId = null, 
            string ipAddress = null, string userAgent = null, string requestBody = null, string responseBody = null);

        /// <summary>
        /// Registra um evento de negócio
        /// </summary>
        /// <param name="eventType">Tipo do evento</param>
        /// <param name="description">Descrição do evento</param>
        /// <param name="entityId">ID da entidade relacionada</param>
        /// <param name="entityType">Tipo da entidade</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="additionalData">Dados adicionais</param>
        void LogBusinessEvent(string eventType, string description, int? entityId = null, string entityType = null, 
            int? userId = null, object additionalData = null);

        /// <summary>
        /// Registra um evento de segurança
        /// </summary>
        /// <param name="eventType">Tipo do evento de segurança</param>
        /// <param name="description">Descrição do evento</param>
        /// <param name="userId">ID do usuário (se aplicável)</param>
        /// <param name="ipAddress">Endereço IP</param>
        /// <param name="userAgent">User Agent</param>
        /// <param name="additionalData">Dados adicionais</param>
        void LogSecurityEvent(string eventType, string description, int? userId = null, string ipAddress = null, 
            string userAgent = null, object additionalData = null);

        /// <summary>
        /// Registra métricas de performance
        /// </summary>
        /// <param name="operation">Nome da operação</param>
        /// <param name="duration">Duração da operação</param>
        /// <param name="success">Se a operação foi bem-sucedida</param>
        /// <param name="additionalMetrics">Métricas adicionais</param>
        void LogPerformance(string operation, TimeSpan duration, bool success = true, Dictionary<string, object> additionalMetrics = null);

        /// <summary>
        /// Registra operações de banco de dados
        /// </summary>
        /// <param name="operation">Tipo da operação (SELECT, INSERT, UPDATE, DELETE)</param>
        /// <param name="table">Nome da tabela</param>
        /// <param name="duration">Duração da operação</param>
        /// <param name="recordsAffected">Número de registros afetados</param>
        /// <param name="query">Query executada (opcional)</param>
        /// <param name="success">Se a operação foi bem-sucedida</param>
        void LogDatabaseOperation(string operation, string table, TimeSpan duration, int recordsAffected = 0, 
            string query = null, bool success = true);

        /// <summary>
        /// Registra chamadas para serviços externos
        /// </summary>
        /// <param name="serviceName">Nome do serviço</param>
        /// <param name="endpoint">Endpoint chamado</param>
        /// <param name="method">Método HTTP</param>
        /// <param name="statusCode">Código de status da resposta</param>
        /// <param name="duration">Duração da chamada</param>
        /// <param name="requestData">Dados da requisição</param>
        /// <param name="responseData">Dados da resposta</param>
        void LogExternalServiceCall(string serviceName, string endpoint, string method, int statusCode, 
            TimeSpan duration, object requestData = null, object responseData = null);

        /// <summary>
        /// Registra um erro
        /// </summary>
        /// <param name="exception">Exceção ocorrida</param>
        /// <param name="context">Contexto onde ocorreu o erro</param>
        /// <param name="userId">ID do usuário (se aplicável)</param>
        /// <param name="additionalData">Dados adicionais</param>
        void LogError(Exception exception, string context = null, int? userId = null, object additionalData = null);

        /// <summary>
        /// Registra informações gerais
        /// </summary>
        /// <param name="message">Mensagem</param>
        /// <param name="context">Contexto</param>
        /// <param name="additionalData">Dados adicionais</param>
        void LogInformation(string message, string context = null, object additionalData = null);

        /// <summary>
        /// Registra um aviso
        /// </summary>
        /// <param name="message">Mensagem de aviso</param>
        /// <param name="context">Contexto</param>
        /// <param name="additionalData">Dados adicionais</param>
        void LogWarning(string message, string context = null, object additionalData = null);

        /// <summary>
        /// Registra informações de debug
        /// </summary>
        /// <param name="message">Mensagem de debug</param>
        /// <param name="context">Contexto</param>
        /// <param name="additionalData">Dados adicionais</param>
        void LogDebug(string message, string context = null, object additionalData = null);

        /// <summary>
        /// Busca logs com filtros
        /// </summary>
        /// <param name="filters">Filtros de busca</param>
        /// <returns>Lista de logs encontrados</returns>
        Task<List<LogEntryDto>> SearchLogsAsync(LogSearchFiltersDto filters);

        /// <summary>
        /// Obtém estatísticas de logs
        /// </summary>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <returns>Estatísticas dos logs</returns>
        Task<LogStatisticsDto> GetLogStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Limpa logs antigos
        /// </summary>
        /// <param name="olderThanDays">Logs mais antigos que X dias</param>
        /// <returns>Número de logs removidos</returns>
        Task<int> CleanupOldLogsAsync(int olderThanDays);
    }
}