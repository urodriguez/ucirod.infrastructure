using System;
using Logging.Domain;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Logging.Application.Dtos
{
    public class LogDtoPost
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