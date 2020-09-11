using Shared.Infrastructure.CrossCuttingV3.Authentication;

namespace Shared.Infrastructure.CrossCuttingV3.Logging
{
    internal class LogDto
    {
        public Credential Credential { get; set; }
        public string Application { get; set; }
        public string Project { get; set; }
        public string CorrelationId { get; set; }
        public string Text { get; set; }
        public LogType Type { get; set; }
        public string Environment { get; set; }
    }
}
