using System.Collections.Generic;
using System.Threading.Tasks;
using Reembolso.Shared.DTOs;

namespace Reembolso.Shared.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Envia um email genérico
        /// </summary>
        /// <param name="to">Destinatário</param>
        /// <param name="subject">Assunto</param>
        /// <param name="body">Corpo do email</param>
        /// <param name="isHtml">Se o corpo é HTML</param>
        /// <param name="attachments">Anexos opcionais</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, List<EmailAttachmentDto> attachments = null);

        /// <summary>
        /// Envia email para múltiplos destinatários
        /// </summary>
        /// <param name="recipients">Lista de destinatários</param>
        /// <param name="subject">Assunto</param>
        /// <param name="body">Corpo do email</param>
        /// <param name="isHtml">Se o corpo é HTML</param>
        /// <param name="attachments">Anexos opcionais</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> SendEmailAsync(List<string> recipients, string subject, string body, bool isHtml = true, List<EmailAttachmentDto> attachments = null);

        /// <summary>
        /// Envia notificação de mudança de status de solicitação de reembolso
        /// </summary>
        /// <param name="userEmail">Email do usuário</param>
        /// <param name="userName">Nome do usuário</param>
        /// <param name="solicitacaoId">ID da solicitação</param>
        /// <param name="novoStatus">Novo status</param>
        /// <param name="observacoes">Observações opcionais</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> SendReimbursementStatusChangeAsync(string userEmail, string userName, int solicitacaoId, string novoStatus, string observacoes = null);

        /// <summary>
        /// Envia notificação de aprovação de solicitação de reembolso
        /// </summary>
        /// <param name="userEmail">Email do usuário</param>
        /// <param name="userName">Nome do usuário</param>
        /// <param name="solicitacaoId">ID da solicitação</param>
        /// <param name="valor">Valor aprovado</param>
        /// <param name="observacoes">Observações opcionais</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> SendReimbursementApprovalAsync(string userEmail, string userName, int solicitacaoId, decimal valor, string observacoes = null);

        /// <summary>
        /// Envia notificação de rejeição de solicitação de reembolso
        /// </summary>
        /// <param name="userEmail">Email do usuário</param>
        /// <param name="userName">Nome do usuário</param>
        /// <param name="solicitacaoId">ID da solicitação</param>
        /// <param name="motivo">Motivo da rejeição</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> SendReimbursementRejectionAsync(string userEmail, string userName, int solicitacaoId, string motivo);

        /// <summary>
        /// Envia notificação de relatório gerado
        /// </summary>
        /// <param name="userEmail">Email do usuário</param>
        /// <param name="userName">Nome do usuário</param>
        /// <param name="relatorioId">ID do relatório</param>
        /// <param name="nomeRelatorio">Nome do relatório</param>
        /// <param name="downloadUrl">URL para download</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> SendReportGeneratedAsync(string userEmail, string userName, int relatorioId, string nomeRelatorio, string downloadUrl);

        /// <summary>
        /// Envia notificação de erro na geração de relatório
        /// </summary>
        /// <param name="userEmail">Email do usuário</param>
        /// <param name="userName">Nome do usuário</param>
        /// <param name="relatorioId">ID do relatório</param>
        /// <param name="nomeRelatorio">Nome do relatório</param>
        /// <param name="erro">Descrição do erro</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> SendReportErrorAsync(string userEmail, string userName, int relatorioId, string nomeRelatorio, string erro);
    }
}