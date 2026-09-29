using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Interfaces
{
    public interface IEmailSender
    {
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, CancellationToken ct = default);
    }
}
