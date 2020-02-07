using System;

namespace Mailing.Domain
{
    public class Sender
    {
        private Sender() { }

        public Sender(string name, string email, string password)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("Field 'Sender.Name' can not be null or empty");
            if (string.IsNullOrEmpty(email)) throw new Exception("Field 'Sender.Email' can not be null or empty");
            if (string.IsNullOrEmpty(password)) throw new Exception("Field 'Sender.Password' can not be null or empty");

            Name = name;
            Email = email;
            Password = password;
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
    }
}