using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reembolso.Shared.DTOs;
using Reembolso.Shared.Interfaces;

namespace Reembolso.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly SmtpClient _smtpClient;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _smtpClient = ConfigureSmtpClient();
        }

        private SmtpClient ConfigureSmtpClient()
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                Timeout = 30000 // 30 segundos
            };

            return client;
        }

        public async Task<bool> EnviarEmailAsync(EmailDto emailDto)
        {
            try
            {
                _logger.LogInformation("Enviando email para {Destinatario} com assunto: {Assunto}", 
                    emailDto.Destinatario, emailDto.Assunto);

                using var message = new MailMessage()
                {
                    From = new MailAddress(_configuration["Email:FromAddress"], _configuration["Email:FromName"]),
                    Subject = emailDto.Assunto,
                    Body = emailDto.Corpo,
                    IsBodyHtml = emailDto.IsHtml
                };

                message.To.Add(emailDto.Destinatario);

                // Adicionar cópias se especificadas
                if (emailDto.Copia?.Count > 0)
                {
                    foreach (var cc in emailDto.Copia)
                    {
                        message.CC.Add(cc);
                    }
                }

                // Adicionar cópias ocultas se especificadas
                if (emailDto.CopiaOculta?.Count > 0)
                {
                    foreach (var bcc in emailDto.CopiaOculta)
                    {
                        message.Bcc.Add(bcc);
                    }
                }

                // Adicionar anexos se especificados
                if (emailDto.Anexos?.Count > 0)
                {
                    foreach (var anexo in emailDto.Anexos)
                    {
                        if (File.Exists(anexo.CaminhoArquivo))
                        {
                            var attachment = new Attachment(anexo.CaminhoArquivo)
                            {
                                Name = anexo.NomeArquivo ?? Path.GetFileName(anexo.CaminhoArquivo)
                            };
                            message.Attachments.Add(attachment);
                        }
                        else
                        {
                            _logger.LogWarning("Anexo não encontrado: {CaminhoArquivo}", anexo.CaminhoArquivo);
                        }
                    }
                }

                await _smtpClient.SendMailAsync(message);
                
                _logger.LogInformation("Email enviado com sucesso para {Destinatario}", emailDto.Destinatario);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Erro SMTP ao enviar email para {Destinatario}: {Erro}", 
                    emailDto.Destinatario, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao enviar email para {Destinatario}: {Erro}", 
                    emailDto.Destinatario, ex.Message);
                return false;
            }
        }

        public async Task<bool> EnviarEmailSolicitacaoReembolsoAsync(string destinatario, string nomeFuncionario, 
            string numeroSolicitacao, decimal valor, string status)
        {
            try
            {
                var assunto = $"Solicitação de Reembolso #{numeroSolicitacao} - {status}";
                var corpo = GerarCorpoEmailSolicitacao(nomeFuncionario, numeroSolicitacao, valor, status);

                var emailDto = new EmailDto
                {
                    Destinatario = destinatario,
                    Assunto = assunto,
                    Corpo = corpo,
                    IsHtml = true
                };

                return await EnviarEmailAsync(emailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de solicitação de reembolso");
                return false;
            }
        }

        public async Task<bool> EnviarEmailAprovacaoAsync(string destinatario, string nomeFuncionario, 
            string numeroSolicitacao, decimal valor, string observacoes = null)
        {
            try
            {
                var assunto = $"Solicitação de Reembolso #{numeroSolicitacao} - APROVADA";
                var corpo = GerarCorpoEmailAprovacao(nomeFuncionario, numeroSolicitacao, valor, observacoes);

                var emailDto = new EmailDto
                {
                    Destinatario = destinatario,
                    Assunto = assunto,
                    Corpo = corpo,
                    IsHtml = true
                };

                return await EnviarEmailAsync(emailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de aprovação");
                return false;
            }
        }

        public async Task<bool> EnviarEmailRejeicaoAsync(string destinatario, string nomeFuncionario, 
            string numeroSolicitacao, decimal valor, string motivoRejeicao)
        {
            try
            {
                var assunto = $"Solicitação de Reembolso #{numeroSolicitacao} - REJEITADA";
                var corpo = GerarCorpoEmailRejeicao(nomeFuncionario, numeroSolicitacao, valor, motivoRejeicao);

                var emailDto = new EmailDto
                {
                    Destinatario = destinatario,
                    Assunto = assunto,
                    Corpo = corpo,
                    IsHtml = true
                };

                return await EnviarEmailAsync(emailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de rejeição");
                return false;
            }
        }

        public async Task<bool> EnviarEmailRelatorioAsync(string destinatario, string nomeRelatorio, 
            string caminhoArquivo, DateTime dataGeracao)
        {
            try
            {
                var assunto = $"Relatório: {nomeRelatorio}";
                var corpo = GerarCorpoEmailRelatorio(nomeRelatorio, dataGeracao);

                var emailDto = new EmailDto
                {
                    Destinatario = destinatario,
                    Assunto = assunto,
                    Corpo = corpo,
                    IsHtml = true,
                    Anexos = new List<AnexoEmailDto>
                    {
                        new AnexoEmailDto
                        {
                            CaminhoArquivo = caminhoArquivo,
                            NomeArquivo = Path.GetFileName(caminhoArquivo)
                        }
                    }
                };

                return await EnviarEmailAsync(emailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de relatório");
                return false;
            }
        }

        private string GerarCorpoEmailSolicitacao(string nomeFuncionario, string numeroSolicitacao, 
            decimal valor, string status)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #2c3e50; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .highlight {{ background-color: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; text-align: center; }}
                        .status {{ font-weight: bold; color: #3498db; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>Sistema de Reembolso</div>
                        </div>
                        <div class='content'>
                            <h2>Solicitação de Reembolso</h2>
                            <p>Olá <strong>{nomeFuncionario}</strong>,</p>
                            <p>Sua solicitação de reembolso foi processada com o status: <span class='status'>{status.ToUpper()}</span></p>
                            
                            <div class='highlight'>
                                <p><strong>Número da Solicitação:</strong> #{numeroSolicitacao}</p>
                                <p><strong>Valor:</strong> R$ {valor:F2}</p>
                                <p><strong>Status:</strong> {status}</p>
                                <p><strong>Data:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                            </div>
                            
                            <p>Para mais detalhes, acesse o sistema de reembolso.</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um email automático. Não responda a esta mensagem.</p>
                            <p>© {DateTime.Now.Year} Sistema de Reembolso. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GerarCorpoEmailAprovacao(string nomeFuncionario, string numeroSolicitacao, 
            decimal valor, string observacoes)
        {
            var observacoesHtml = string.IsNullOrEmpty(observacoes) ? "" : 
                $"<p><strong>Observações:</strong> {observacoes}</p>";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #2c3e50; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .highlight {{ background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; text-align: center; }}
                        .approved {{ font-weight: bold; color: #28a745; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>Sistema de Reembolso</div>
                        </div>
                        <div class='content'>
                            <h2>✅ Solicitação Aprovada!</h2>
                            <p>Olá <strong>{nomeFuncionario}</strong>,</p>
                            <p>Temos uma ótima notícia! Sua solicitação de reembolso foi <span class='approved'>APROVADA</span>.</p>
                            
                            <div class='highlight'>
                                <p><strong>Número da Solicitação:</strong> #{numeroSolicitacao}</p>
                                <p><strong>Valor Aprovado:</strong> R$ {valor:F2}</p>
                                <p><strong>Data de Aprovação:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                                {observacoesHtml}
                            </div>
                            
                            <p>O valor será processado para pagamento em breve. Você receberá uma confirmação quando o pagamento for efetuado.</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um email automático. Não responda a esta mensagem.</p>
                            <p>© {DateTime.Now.Year} Sistema de Reembolso. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GerarCorpoEmailRejeicao(string nomeFuncionario, string numeroSolicitacao, 
            decimal valor, string motivoRejeicao)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #2c3e50; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .highlight {{ background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #dc3545; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; text-align: center; }}
                        .rejected {{ font-weight: bold; color: #dc3545; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>Sistema de Reembolso</div>
                        </div>
                        <div class='content'>
                            <h2>❌ Solicitação Rejeitada</h2>
                            <p>Olá <strong>{nomeFuncionario}</strong>,</p>
                            <p>Infelizmente, sua solicitação de reembolso foi <span class='rejected'>REJEITADA</span>.</p>
                            
                            <div class='highlight'>
                                <p><strong>Número da Solicitação:</strong> #{numeroSolicitacao}</p>
                                <p><strong>Valor:</strong> R$ {valor:F2}</p>
                                <p><strong>Data de Rejeição:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                                <p><strong>Motivo da Rejeição:</strong> {motivoRejeicao}</p>
                            </div>
                            
                            <p>Se você acredita que houve um erro ou tem dúvidas sobre a rejeição, entre em contato com o departamento responsável.</p>
                            <p>Você pode submeter uma nova solicitação com as correções necessárias.</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um email automático. Não responda a esta mensagem.</p>
                            <p>© {DateTime.Now.Year} Sistema de Reembolso. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GerarCorpoEmailRelatorio(string nomeRelatorio, DateTime dataGeracao)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #2c3e50; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .highlight {{ background-color: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>Sistema de Reembolso</div>
                        </div>
                        <div class='content'>
                            <h2>📊 Relatório Disponível</h2>
                            <p>Seu relatório foi gerado com sucesso!</p>
                            
                            <div class='highlight'>
                                <p><strong>Nome do Relatório:</strong> {nomeRelatorio}</p>
                                <p><strong>Data de Geração:</strong> {dataGeracao:dd/MM/yyyy HH:mm}</p>
                            </div>
                            
                            <p>O relatório está anexado a este email. Você também pode acessá-lo através do sistema.</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um email automático. Não responda a esta mensagem.</p>
                            <p>© {DateTime.Now.Year} Sistema de Reembolso. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}