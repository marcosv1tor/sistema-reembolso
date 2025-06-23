using System;
using System.Collections.Generic;

namespace Reembolso.Shared.DTOs
{
    /// <summary>
    /// DTO para resultado de upload de arquivo
    /// </summary>
    public class FileUploadResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string RelativePath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string FileExtension { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FileHash { get; set; }
        public string ErrorDetails { get; set; }
    }

    /// <summary>
    /// DTO para informações de arquivo
    /// </summary>
    public class FileInfoDto
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string RelativePath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string FileExtension { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime AccessedAt { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public string FileHash { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// DTO para resultado de validação de arquivo
    /// </summary>
    public class FileValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string FileExtension { get; set; }
        public bool SizeValid { get; set; }
        public bool ExtensionValid { get; set; }
        public bool ContentTypeValid { get; set; }
        public bool FileNameValid { get; set; }
    }

    /// <summary>
    /// DTO para configuração de upload
    /// </summary>
    public class FileUploadConfigDto
    {
        public string[] AllowedExtensions { get; set; }
        public string[] BlockedExtensions { get; set; }
        public long MaxFileSize { get; set; } = 10485760; // 10MB
        public long MaxTotalSize { get; set; } = 104857600; // 100MB
        public int MaxFileCount { get; set; } = 10;
        public string UploadDirectory { get; set; }
        public bool CreateDirectoryIfNotExists { get; set; } = true;
        public bool OverwriteExistingFiles { get; set; } = false;
        public bool GenerateUniqueNames { get; set; } = true;
        public bool ValidateContentType { get; set; } = true;
        public bool ScanForViruses { get; set; } = false;
        public bool CompressImages { get; set; } = false;
        public int ImageMaxWidth { get; set; } = 1920;
        public int ImageMaxHeight { get; set; } = 1080;
        public int ImageQuality { get; set; } = 85;
    }

    /// <summary>
    /// DTO para estatísticas de arquivos
    /// </summary>
    public class FileStatisticsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int SuccessfulUploads { get; set; }
        public int FailedUploads { get; set; }
        public double SuccessRate { get; set; }
        public long AverageFileSize { get; set; }
        public Dictionary<string, int> FilesByExtension { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, long> SizeByExtension { get; set; } = new Dictionary<string, long>();
        public Dictionary<DateTime, int> UploadsByDay { get; set; } = new Dictionary<DateTime, int>();
        public Dictionary<string, int> FailureReasons { get; set; } = new Dictionary<string, int>();
        public List<string> LargestFiles { get; set; } = new List<string>();
        public List<string> MostRecentFiles { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO para filtros de busca de arquivos
    /// </summary>
    public class FileSearchFiltersDto
    {
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public long? MinSize { get; set; }
        public long? MaxSize { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? ModifiedAfter { get; set; }
        public DateTime? ModifiedBefore { get; set; }
        public string Directory { get; set; }
        public bool IncludeSubdirectories { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// DTO para operação de limpeza de arquivos
    /// </summary>
    public class FileCleanupResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int FilesRemoved { get; set; }
        public long SpaceFreed { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> RemovedFiles { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime ExecutedAt { get; set; }
    }

    /// <summary>
    /// DTO para backup de arquivos
    /// </summary>
    public class FileBackupDto
    {
        public string BackupId { get; set; }
        public string SourceDirectory { get; set; }
        public string BackupDirectory { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FileCount { get; set; }
        public long TotalSize { get; set; }
        public TimeSpan Duration { get; set; }
        public BackupStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> BackedUpFiles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Enum para status de backup
    /// </summary>
    public enum BackupStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    /// <summary>
    /// DTO para quota de arquivos por usuário
    /// </summary>
    public class UserFileQuotaDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public long QuotaLimit { get; set; }
        public long CurrentUsage { get; set; }
        public double UsagePercentage { get; set; }
        public int FileCount { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsOverQuota { get; set; }
        public long AvailableSpace { get; set; }
    }
}