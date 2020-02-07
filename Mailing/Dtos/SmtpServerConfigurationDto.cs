namespace Mailing.Dtos
{
    public class SmtpServerConfigurationDto
    {
        public SenderDto Sender { get; set; }
        public HostDto Host { get; set; }
    }
}