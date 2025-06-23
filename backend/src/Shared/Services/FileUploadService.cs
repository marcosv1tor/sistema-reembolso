using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reembolso.Shared.DTOs;
using Reembolso.Shared.Interfaces;

namespace Reembolso.Shared.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly ILogger<FileUploadService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadPath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;
        private readonly string[] _allowedMimeTypes;

        public FileUploadService(ILogger<FileUploadService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            _uploadPath = _configuration["FileUpload:UploadPath"] ?? "uploads";
            _maxFileSize = long.Parse(_configuration["FileUpload:MaxFileSize"] ?? "10485760"); // 10MB default
            _allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() 
                ?? new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
            _allowedMimeTypes = _configuration.GetSection("FileUpload:AllowedMimeTypes").Get<string[]>()
                ?? new[] { 
                    "application/pdf", 
                    "image/jpeg", 
                    "image/png", 
                    "image/gif", 
                    "application/msword", 
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "text/plain"
                };

            // Criar diretório de upload se não existir
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<UploadResultDto> UploadFileAsync(IFormFile file, string subDirectory = null)
        {
            try
            {
                // Validar arquivo
                var validationResult = ValidateFile(file);
                if (!validationResult.IsValid)
                {
                    return new UploadResultDto
                    {
                        Success = false,
                        ErrorMessage = validationResult.ErrorMessage
                    };
                }

                // Gerar nome único para o arquivo
                var fileName = GenerateUniqueFileName(file.FileName);
                
                // Determinar caminho de destino
                var destinationPath = _uploadPath;
                if (!string.IsNullOrEmpty(subDirectory))
                {
                    destinationPath = Path.Combine(_uploadPath, subDirectory);
                    if (!Directory.Exists(destinationPath))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }
                }

                var filePath = Path.Combine(destinationPath, fileName);

                // Salvar arquivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Arquivo {FileName} salvo com sucesso em {FilePath}", 
                    file.FileName, filePath);

                return new UploadResultDto
                {
                    Success = true,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploadDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload do arquivo {FileName}", file.FileName);
                return new UploadResultDto
                {
                    Success = false,
                    ErrorMessage = "Erro interno ao processar o arquivo"
                };
            }
        }

        public async Task<List<UploadResultDto>> UploadMultipleFilesAsync(IFormFileCollection files, string subDirectory = null)
        {
            var results = new List<UploadResultDto>();

            foreach (var file in files)
            {
                var result = await UploadFileAsync(file, subDirectory);
                results.Add(result);
            }

            return results;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation("Arquivo {FilePath} excluído com sucesso", filePath);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Tentativa de excluir arquivo inexistente: {FilePath}", filePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir arquivo {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> DeleteMultipleFilesAsync(IEnumerable<string> filePaths)
        {
            var allDeleted = true;

            foreach (var filePath in filePaths)
            {
                var deleted = await DeleteFileAsync(filePath);
                if (!deleted)
                {
                    allDeleted = false;
                }
            }

            return allDeleted;
        }

        public FileInfoDto GetFileInfo(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var fileInfo = new FileInfo(filePath);
                return new FileInfoDto
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    CreationDate = fileInfo.CreationTime,
                    LastModified = fileInfo.LastWriteTime,
                    Extension = fileInfo.Extension
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do arquivo {FilePath}", filePath);
                return null;
            }
        }

        public async Task<byte[]> GetFileContentAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler conteúdo do arquivo {FilePath}", filePath);
                return null;
            }
        }

        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao abrir stream do arquivo {FilePath}", filePath);
                return null;
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public long GetDirectorySize(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return 0;
                }

                var directoryInfo = new DirectoryInfo(directoryPath);
                return directoryInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular tamanho do diretório {DirectoryPath}", directoryPath);
                return 0;
            }
        }

        public async Task<bool> CleanupOldFilesAsync(string directoryPath, TimeSpan maxAge)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return true;
                }

                var cutoffDate = DateTime.UtcNow - maxAge;
                var directoryInfo = new DirectoryInfo(directoryPath);
                var oldFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                    .Where(file => file.CreationTimeUtc < cutoffDate)
                    .ToList();

                var deletedCount = 0;
                foreach (var file in oldFiles)
                {
                    try
                    {
                        file.Delete();
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao excluir arquivo antigo {FilePath}", file.FullName);
                    }
                }

                _logger.LogInformation("Limpeza concluída: {DeletedCount} arquivos antigos removidos de {DirectoryPath}", 
                    deletedCount, directoryPath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante limpeza de arquivos antigos em {DirectoryPath}", directoryPath);
                return false;
            }
        }

        public string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".xml" => "application/xml",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }

        private FileValidationResult ValidateFile(IFormFile file)
        {
            // Verificar se o arquivo existe
            if (file == null || file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Nenhum arquivo foi enviado"
                };
            }

            // Verificar tamanho do arquivo
            if (file.Length > _maxFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Arquivo muito grande. Tamanho máximo permitido: {_maxFileSize / 1024 / 1024} MB"
                };
            }

            // Verificar extensão do arquivo
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Tipo de arquivo não permitido. Extensões permitidas: {string.Join(", ", _allowedExtensions)}"
                };
            }

            // Verificar MIME type
            if (!_allowedMimeTypes.Contains(file.ContentType))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Tipo de conteúdo não permitido"
                };
            }

            // Verificar nome do arquivo
            if (string.IsNullOrWhiteSpace(file.FileName) || file.FileName.Length > 255)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Nome do arquivo inválido"
                };
            }

            // Verificar caracteres perigosos no nome do arquivo
            var invalidChars = Path.GetInvalidFileNameChars();
            if (file.FileName.Any(c => invalidChars.Contains(c)))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Nome do arquivo contém caracteres inválidos"
                };
            }

            return new FileValidationResult { IsValid = true };
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8]; // Primeiros 8 caracteres do GUID
            
            return $"{nameWithoutExtension}_{timestamp}_{guid}{extension}";
        }

        private class FileValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}