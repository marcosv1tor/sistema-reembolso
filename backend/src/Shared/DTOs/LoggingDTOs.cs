using System;
using System.Collections.Generic;

namespace Reembolso.Shared.DTOs
{
    /// <summary>
    /// DTO para entrada de log
    /// </summary>
    public class LogEntryDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public string Category { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public string ServiceName { get; set; }
        public string Environment { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public string Exception { get; set; }
        public string StackTrace { get; set; }
    }

    /// <summary>
    /// DTO para filtros de busca de logs
    /// </summary>
    public class LogSearchFiltersDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Levels { get; set; } = new List<string>();
        public string Message { get; set; }
        public string Context { get; set; }
        public string Category { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public string ServiceName { get; set; }
        public string Environment { get; set; }
        public bool HasException { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public string SortBy { get; set; } = "Timestamp";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// DTO para estatísticas de logs
    /// </summary>
    public class LogStatisticsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long TotalLogs { get; set; }
        public Dictionary<string, long> LogsByLevel { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, long> LogsByCategory { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, long> LogsByService { get; set; } = new Dictionary<string, long>();
        public Dictionary<DateTime, long> LogsByHour { get; set; } = new Dictionary<DateTime, long>();
        public Dictionary<DateTime, long> LogsByDay { get; set; } = new Dictionary<DateTime, long>();
        public long ErrorCount { get; set; }
        public long WarningCount { get; set; }
        public long InfoCount { get; set; }
        public long DebugCount { get; set; }
        public double ErrorRate { get; set; }
        public List<TopErrorDto> TopErrors { get; set; } = new List<TopErrorDto>();
        public List<TopUserDto> TopUsers { get; set; } = new List<TopUserDto>();
        public List<TopIpDto> TopIpAddresses { get; set; } = new List<TopIpDto>();
    }

    /// <summary>
    /// DTO para top erros
    /// </summary>
    public class TopErrorDto
    {
        public string Message { get; set; }
        public string Exception { get; set; }
        public long Count { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
        public List<string> AffectedServices { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO para top usuários
    /// </summary>
    public class TopUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public long LogCount { get; set; }
        public long ErrorCount { get; set; }
        public DateTime LastActivity { get; set; }
        public List<string> TopActions { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO para top IPs
    /// </summary>
    public class TopIpDto
    {
        public string IpAddress { get; set; }
        public long RequestCount { get; set; }
        public long ErrorCount { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public List<string> UserAgents { get; set; } = new List<string>();
        public bool IsSuspicious { get; set; }
    }

    /// <summary>
    /// DTO para configuração de logging
    /// </summary>
    public class LoggingConfigDto
    {
        public string MinimumLevel { get; set; } = "Information";
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnableFileLogging { get; set; } = true;
        public bool EnableElasticsearchLogging { get; set; } = false;
        public bool EnableDatabaseLogging { get; set; } = false;
        public string LogDirectory { get; set; } = "logs";
        public string LogFilePattern { get; set; } = "log-{Date}.txt";
        public long MaxLogFileSize { get; set; } = 104857600; // 100MB
        public int RetainedFileCountLimit { get; set; } = 30;
        public string ElasticsearchUrl { get; set; }
        public string ElasticsearchIndex { get; set; } = "logs";
        public bool IncludeScopes { get; set; } = true;
        public bool IncludeStackTrace { get; set; } = true;
        public List<string> SensitiveProperties { get; set; } = new List<string> { "password", "token", "key", "secret" };
    }

    /// <summary>
    /// DTO para evento de auditoria
    /// </summary>
    public class AuditEventDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public string ServiceName { get; set; }
    }

    /// <summary>
    /// DTO para métricas de performance
    /// </summary>
    public class PerformanceMetricDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; }
        public string Category { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        public string ServiceName { get; set; }
        public string RequestId { get; set; }
        public int? UserId { get; set; }
    }

    /// <summary>
    /// DTO para evento de segurança
    /// </summary>
    public class SecurityEventDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string RequestId { get; set; }
        public string ServiceName { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
        public bool IsBlocked { get; set; }
        public string Action { get; set; }
    }

    /// <summary>
    /// DTO para alerta de log
    /// </summary>
    public class LogAlertDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string Level { get; set; }
        public int ThresholdCount { get; set; }
        public TimeSpan TimeWindow { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; }
        public List<string> NotificationEmails { get; set; } = new List<string>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// DTO para resultado de busca paginada de logs
    /// </summary>
    public class LogSearchResultDto
    {
        public List<LogEntryDto> Logs { get; set; } = new List<LogEntryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }
}