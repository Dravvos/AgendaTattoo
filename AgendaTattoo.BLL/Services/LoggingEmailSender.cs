using AgendaTattoo.BLL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Services
{
    public class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, CancellationToken ct = default)
        {
            _logger.LogWarning(
                "[DEV] E-mail de redefinição de senha NÃO enviado (sem provedor de e-mail configurado). " +
                "Destinatário: {Email} ({Name}). Link: {ResetLink}",
                toEmail, toName, resetLink);

            return Task.CompletedTask;
        }

    }
}
