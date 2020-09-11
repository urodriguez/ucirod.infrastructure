using System.Collections.Generic;
using Shared.Infrastructure.CrossCuttingV3.Authentication;

namespace Mailing.Dtos
{
    public class EmailDto
    {
        public Credential Credential { get; set; }
        public bool UseCustomSmtpServer { get; set; }
        public SmtpServerConfigurationDto SmtpServerConfiguration { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IEnumerable<AttachmentDto> Attachments { get; set; }
    }
}