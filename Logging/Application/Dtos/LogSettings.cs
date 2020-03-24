using System;

namespace Logging.Application.Dtos
{
    public class LogSettings
    {
        public string Application { get; set; }
        public string Project { get; set; }
        public string Environment { get; set; }
        public Guid CorrelationId { get; set; }
    }
}