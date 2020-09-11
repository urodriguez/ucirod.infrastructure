using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Mailing.Domain;
using Mailing.Dtos;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Shared.Infrastructure.CrossCuttingV3.Authentication;
using Shared.Infrastructure.CrossCuttingV3.Logging;
using Shared.WebApiV3.Controllers;
using Attachment = Mailing.Domain.Attachment;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Mailing.Controllers
{
    public class EmailsController : InfrastructureController
    {
        public EmailsController(ICredentialService credentialService, ILogService logService) : base(credentialService, logService)
        {
        }

        protected override void ConfigureLogging()
        {
            _logService.UseProject("Mailing");
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] EmailDto emailDto)
        {
            return await ExecuteAsync(emailDto.Credential, async () =>
            {
                Email email;

                if (emailDto.UseCustomSmtpServer)
                {
                    _logService.LogInfoMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Using Custom SmtpServer | emailDto.To={emailDto.To}");

                    if (emailDto.SmtpServerConfiguration == null || emailDto.SmtpServerConfiguration.Sender == null || emailDto.SmtpServerConfiguration.Host == null)
                    {
                        const string msg = "Missing required data on SmtpServerConfiguration when UseCustomSmtpServer is enable";
                        _logService.LogErrorMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Invalid request data| msg={msg} - emailDto.To={emailDto.To}");
                        throw new ArgumentNullException(msg);
                    }
                    
                    email = new Email(
                        emailDto.SmtpServerConfiguration.Sender.Name,
                        emailDto.SmtpServerConfiguration.Sender.Email,
                        emailDto.SmtpServerConfiguration.Sender.Password,
                        emailDto.SmtpServerConfiguration.Host.Name,
                        emailDto.SmtpServerConfiguration.Host.Port,
                        emailDto.SmtpServerConfiguration.Host.UseSsl,
                        emailDto.To,
                        emailDto.Subject,
                        emailDto.Body
                    );
                }
                else
                {
                    _logService.LogInfoMessageAsync($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Using UciRod SmtpServer | emailDto.To={emailDto.To}");
                    email = new Email(
                        emailDto.To,
                        emailDto.Subject,
                        emailDto.Body
                    );
                }

                if (emailDto.Attachments != null)
                {
                    foreach (var attachment in emailDto.Attachments)
                    {
                        email.AddAttachment(attachment.FileContent, attachment.FileName);
                    }
                }

                var mimeMessage = new MimeMessage();

                mimeMessage.From.Add(new MailboxAddress(email.SmtpConfiguration.Sender.Name, email.SmtpConfiguration.Sender.Email));

                mimeMessage.To.Add(new MailboxAddress("", email.To));

                mimeMessage.Subject = email.Subject;

                SetBody(mimeMessage, email.Body, email.Attachments);

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    client.Timeout = 10000;

                    _logService.LogInfoMessageAsync(
                        $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Connecting with host | email.To={email.To} - host.name={email.SmtpConfiguration.Host.Name}"
                    );
                    // The third parameter is useSSL ('true' if the client should make an SSL-wrapped connection to the server)
                    await client.ConnectAsync(email.SmtpConfiguration.Host.Name, email.SmtpConfiguration.Host.Port, email.SmtpConfiguration.Host.UseSsl);

                    _logService.LogInfoMessageAsync(
                        $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | " +
                        $"Authenticating into host | " +
                        $"email.To={email.To} - host.name={email.SmtpConfiguration.Host.Name} - sender.email={email.SmtpConfiguration.Sender.Email}"
                    );
                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(email.SmtpConfiguration.Sender.Email, email.SmtpConfiguration.Sender.Password);

                    _logService.LogInfoMessageAsync(
                        $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | " +
                        $"Sending email | " +
                        $"email.To={email.To} - STATUS=PENDING"
                    );

                    await client.SendAsync(mimeMessage);

                    _logService.LogInfoMessageAsync(
                        $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | " +
                        $"Email sent | " +
                        $"email.To={email.To} - STATUS=OK"
                    );

                    await client.DisconnectAsync(true);

                    return Ok();
                }
            });
        }

        private static void SetBody(MimeMessage mimeMessage, string body, IEnumerable<Attachment> attachments)
        {
            var builder = new BodyBuilder { HtmlBody = body };

            foreach (var attachment in attachments)
            {
                builder.Attachments.Add(attachment.FileName, attachment.FileContent);
            }

            mimeMessage.Body = builder.ToMessageBody();
        }
    }
}
