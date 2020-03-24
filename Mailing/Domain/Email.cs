using System;

namespace Mailing.Domain
{
    public class Email
    {
        private Email() {}

        public Email(string to, string subject, string body)
        {
            SmtpConfiguration = new SmtpServerConfiguration("UciRod", "ucirod.infrastructure@gmail.com", "uc1r0d.1nfr4structur3", "smtp.gmail.com", 465, true);

            if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("Field 'to' can not be null or empty");

            To = to;
            Subject = subject;
            Body = body;
        }

        public Email(
            string senderName,
            string senderEmail,
            string senderPassword,
            string hostName,
            int hostPort,
            bool hostUseSsl,
            string to,
            string subject,
            string body
        )
        {
            SmtpConfiguration = new SmtpServerConfiguration(senderName, senderEmail, senderPassword, hostName, hostPort, hostUseSsl);

            if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("Field 'to' can not be null or empty");

            To = to;
            Subject = subject;
            Body = body;
        }

        public SmtpServerConfiguration SmtpConfiguration { get; private set; }
        public string To { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
    }
}