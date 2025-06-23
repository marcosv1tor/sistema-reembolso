using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reembolso.Shared.DTOs;

namespace Reembolso.Shared.Interfaces
{
    public interface IFileUploadService
    {
        /// <summary>
        /// Faz upload de um único arquivo
        /// </summary>
        /// <param name="file">Arquivo a ser enviado</param>
        /// <param name="directory">Diretório de destino</param>
        /// <param name="allowedExtensions">Extensões permitidas</param>
        /// <param name="maxSizeBytes">Tamanho máximo em bytes</param>
        /// <returns>Informações do arquivo enviado</returns>
        Task<FileUploadResultDto> UploadFileAsync(IFormFile file, string directory, string[] allowedExtensions = null, long maxSizeBytes = 10485760);

        /// <summary>
        /// Faz upload de múltiplos arquivos
        /// </summary>
        /// <param name="files">Lista de arquivos a serem enviados</param>
        /// <param name="directory">Diretório de destino</param>
        /// <param name="allowedExtensions">Extensões permitidas</param>
        /// <param name="maxSizeBytes">Tamanho máximo por arquivo em bytes</param>
        /// <returns>Lista com informações dos arquivos enviados</returns>
        Task<List<FileUploadResultDto>> UploadFilesAsync(IFormFileCollection files, string directory, string[] allowedExtensions = null, long maxSizeBytes = 10485760);

        /// <summary>
        /// Deleta um arquivo
        /// </summary>
        /// <param name="filePath">Caminho do arquivo</param>
        /// <returns>True se deletado com sucesso</returns>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// Deleta múltiplos arquivos
        /// </summary>
        /// <param name="filePaths">Lista de caminhos dos arquivos</param>
        /// <returns>Número de arquivos deletados com sucesso</returns>
        Task<int> DeleteFilesAsync(List<string> filePaths);

        /// <summary>
        /// Obtém informações de um arquivo
        /// </summary>
        /// <param name="filePath">Caminho do arquivo</param>
        /// <returns>Informações do arquivo</returns>
        Task<FileInfoDto> GetFileInfoAsync(string filePath);

        /// <summary>
        /// Obtém o conteúdo de um arquivo como array de bytes
        /// </summary>
        /// <param name="filePath">Caminho do arquivo</param>
        /// <returns>Conteúdo do arquivo</returns>
        Task<byte[]> GetFileContentAsync(string filePath);

        /// <summary>
        /// Obtém um stream do arquivo
        /// </summary>
        /// <param name="filePath">Caminho do arquivo</param>
        /// <returns>Stream do arquivo</returns>
        Task<Stream> GetFileStreamAsync(string filePath);

        /// <summary>
        /// Verifica se um arquivo existe
        /// </summary>
        /// <param name="filePath">Caminho do arquivo</param>
        /// <returns>True se o arquivo existe</returns>
        Task<bool> FileExistsAsync(string filePath);

        /// <summary>
        /// Calcula o tamanho total de um diretório
        /// </summary>
        /// <param name="directoryPath">Caminho do diretório</param>
        /// <returns>Tamanho total em bytes</returns>
        Task<long> GetDirectorySizeAsync(string directoryPath);

        /// <summary>
        /// Lista arquivos em um diretório
        /// </summary>
        /// <param name="directoryPath">Caminho do diretório</param>
        /// <param name="searchPattern">Padrão de busca</param>
        /// <param name="includeSubdirectories">Incluir subdiretórios</param>
        /// <returns>Lista de informações dos arquivos</returns>
        Task<List<FileInfoDto>> ListFilesAsync(string directoryPath, string searchPattern = "*", bool includeSubdirectories = false);

        /// <summary>
        /// Limpa arquivos antigos de um diretório
        /// </summary>
        /// <param name="directoryPath">Caminho do diretório</param>
        /// <param name="olderThanDays">Arquivos mais antigos que X dias</param>
        /// <returns>Número de arquivos removidos</returns>
        Task<int> CleanupOldFilesAsync(string directoryPath, int olderThanDays);

        /// <summary>
        /// Valida um arquivo antes do upload
        /// </summary>
        /// <param name="file">Arquivo a ser validado</param>
        /// <param name="allowedExtensions">Extensões permitidas</param>
        /// <param name="maxSizeBytes">Tamanho máximo em bytes</param>
        /// <returns>Resultado da validação</returns>
        Task<FileValidationResultDto> ValidateFileAsync(IFormFile file, string[] allowedExtensions = null, long maxSizeBytes = 10485760);

        /// <summary>
        /// Obtém o tipo MIME de um arquivo
        /// </summary>
        /// <param name="fileName">Nome do arquivo</param>
        /// <returns>Tipo MIME</returns>
        string GetMimeType(string fileName);

        /// <summary>
        /// Gera um nome único para um arquivo
        /// </summary>
        /// <param name="originalFileName">Nome original do arquivo</param>
        /// <returns>Nome único gerado</returns>
        string GenerateUniqueFileName(string originalFileName);

        /// <summary>
        /// Cria um diretório se não existir
        /// </summary>
        /// <param name="directoryPath">Caminho do diretório</param>
        /// <returns>True se criado ou já existe</returns>
        Task<bool> EnsureDirectoryExistsAsync(string directoryPath);
    }
}