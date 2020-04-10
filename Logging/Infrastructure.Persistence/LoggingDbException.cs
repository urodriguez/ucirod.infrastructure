using System;

namespace Logging.Infrastructure.Persistence
{
    public class LoggingDbException : Exception
    {
        public LoggingDbException(Exception inner) : base("An error has ocurred on LoggingDb", inner)
        {
        }
    }
}