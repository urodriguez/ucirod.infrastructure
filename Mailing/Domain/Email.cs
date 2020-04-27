using System;
using System.Collections.Generic;

namespace Mailing.Domain
{
    public class Email
    {
        private Email() {}

        public Email(string to, string subject, string body)
        {
            SetEmailMetadata(
                new SmtpServerConfiguration("UciRod", "ucirod.infrastructure@gmail.com", "uc1r0d.1nfr4structur3", "smtp.gmail.com", 465, true),
                to,
                subject,
                body
            );
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
            SetEmailMetadata(
                new SmtpServerConfiguration(senderName, senderEmail, senderPassword, hostName, hostPort, hostUseSsl),
                to,
                subject,
                body
            );
        }

        private void SetEmailMetadata(SmtpServerConfiguration smtpServerConfiguration, string to, string subject, string body)
        {
            SmtpConfiguration = smtpServerConfiguration;

            if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("Field 'to' can not be null or empty");

            To = to;
            Subject = subject;
            Body = body;
            Attachments = new List<Attachment>();
        }

        public SmtpServerConfiguration SmtpConfiguration { get; private set; }
        public string To { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public IList<Attachment> Attachments { get; private set; }

        public void AddAttachment(byte[] fileContent, string fileName)
        {
            Attachments.Add(new Attachment(fileContent, fileName));
        }
    }
}