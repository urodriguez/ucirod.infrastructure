using Infrastructure.CrossCutting.Authentication;

namespace Mailing.Dtos
{
    public class EmailDto
    {
        public Account Account { get; set; }
        public bool UseCustomSmtpServer { get; set; }
        public SmtpServerConfigurationDto SmtpServerConfiguration { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}