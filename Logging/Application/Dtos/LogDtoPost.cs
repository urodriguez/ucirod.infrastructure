using System;
using Infrastructure.CrossCutting.Authentication;
using Logging.Domain;

namespace Logging.Application.Dtos
{
    public class LogDtoPost
    {
        public Account Account { get; set; }
        public string Application { get; set; }
        public string Project { get; set; }
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
        public LogType Type { get; set; }
        public string Environment { get; set; }
    }
}