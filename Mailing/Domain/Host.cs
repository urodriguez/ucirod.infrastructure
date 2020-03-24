using System;

namespace Mailing.Domain
{
    public class Host
    {
        private Host() { }

        public Host(string name, int port, bool useSsl)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Field 'Host.Name' can not be null or empty");
            if (port <= 0) throw new ArgumentOutOfRangeException("Field 'Host.Port' can not be less or equal than zero");

            Name = name;
            Port = port;
            UseSsl = useSsl;
        }

        public string Name { get; private set; }
        public int Port { get; private set; }
        public bool UseSsl { get; private set; }
    }
}