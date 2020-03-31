using System;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Shared.Infrastructure.CrossCutting.Logging
{
    internal class LogDto
    {
        public Credential Credential { get; set; }
        public string Application { get; set; }
        public string Project { get; set; }
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
        public LogType Type { get; set; }
        public string Environment { get; set; }
    }
}
