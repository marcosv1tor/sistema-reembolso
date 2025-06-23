using System;
using System.Collections.Generic;

namespace Reembolso.Shared.DTOs
{
    /// <summary>
    /// DTO para anexo de email
    /// </summary>
    public class EmailAttachmentDto
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }

    /// <summary>
    /// DTO para configuração de email
    /// </summary>
    public class EmailConfigDto
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
    }

    /// <summary>
    /// DTO para resultado de envio de email
    /// </summary>
    public class EmailResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public string MessageId { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public string Subject { get; set; }
        public int AttachmentCount { get; set; }
        public TimeSpan Duration { get; set; }
        public string ErrorDetails { get; set; }
    }

    /// <summary>
    /// DTO para template de email
    /// </summary>
    public class EmailTemplateDto
    {
        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
        public List<string> RequiredParameters { get; set; } = new List<string>();
        public Dictionary<string, object> DefaultParameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// DTO para fila de emails
    /// </summary>
    public class EmailQueueDto
    {
        public int Id { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public List<EmailAttachmentDto> Attachments { get; set; } = new List<EmailAttachmentDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public DateTime? SentAt { get; set; }
        public EmailStatus Status { get; set; }
        public int RetryCount { get; set; }
        public string ErrorMessage { get; set; }
        public int Priority { get; set; } = 5; // 1 = Alta, 5 = Normal, 10 = Baixa
    }

    /// <summary>
    /// DTO para estatísticas de email
    /// </summary>
    public class EmailStatisticsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int TotalPending { get; set; }
        public double SuccessRate { get; set; }
        public double AverageDeliveryTimeMs { get; set; }
        public Dictionary<string, int> FailureReasons { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> EmailsByTemplate { get; set; } = new Dictionary<string, int>();
        public Dictionary<DateTime, int> EmailsByDay { get; set; } = new Dictionary<DateTime, int>();
    }

    /// <summary>
    /// Enum para status de email
    /// </summary>
    public enum EmailStatus
    {
        Pending = 0,
        Sending = 1,
        Sent = 2,
        Failed = 3,
        Cancelled = 4,
        Scheduled = 5
    }

    /// <summary>
    /// DTO para filtros de busca de emails
    /// </summary>
    public class EmailSearchFiltersDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EmailStatus? Status { get; set; }
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public int? Priority { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}