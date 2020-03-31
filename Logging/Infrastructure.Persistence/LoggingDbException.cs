using System;

namespace Logging.Infrastructure.Persistence
{
    public class LoggingDbException : Exception
    {
        public string OriginalStackTrace { get; set; }

        public LoggingDbException(string stackTrace) : base("An error on LoggingDb has ocurred")
        {
            OriginalStackTrace = stackTrace;
        }
    }
}