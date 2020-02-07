namespace Mailing.Domain
{
    public class SmtpServerConfiguration
    {
        private SmtpServerConfiguration() {}

        public SmtpServerConfiguration(string name, string email, string password, string hostName, int port, bool useSsl)
        {
            Sender = new Sender(name, email, password);
            Host = new Host(hostName, port, useSsl);
        }

        public Sender Sender { get; private set; }
        public Host Host { get; private set; }
    }
}