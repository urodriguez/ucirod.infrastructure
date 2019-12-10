using System;

namespace Infrastructure.CrossCutting.LogService
{
    public class Log
    {
        public string Application { get; set; }
        public string Project { get; set; }
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
        public LogType Type { get; set; }
    }
}