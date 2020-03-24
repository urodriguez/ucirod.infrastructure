using System;
using System.Linq;

namespace Logging.Domain
{
    public class Log
    {
        public Log(string application, string project, Guid correlationId, string text, LogType type, string environment)
        {
            if (string.IsNullOrEmpty(application)) throw new ArgumentNullException("Application is missing on Log object");
            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException("Environment is missing on Log object");

            if (!Enum.IsDefined(typeof(LogType), type))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(LogType)).Select(lt => lt));
                throw new ArgumentOutOfRangeException($"Invalid Log Type on Log request object. Valid types are = {validTypes}");
            }

            Id = Guid.NewGuid();
            Application = application;
            Project = project;
            CorrelationId = correlationId;
            Text = text;
            Type = type;
            Environment = environment;
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public string Application { get; set; }
        public string Project { get; set; }
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
        public LogType Type { get; set; }
        public string Environment { get; set; }
        public DateTime CreationDate { get; set; }
    }
}