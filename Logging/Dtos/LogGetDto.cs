using System;
using Logging.Domain;

namespace Logging.Dtos
{
    public class LogGetDto
    {
        public LogGetDto(Log ol)
        {
            Application = ol.Application;
            Project = ol.Project;
            CorrelationId = ol.CorrelationId;
            Text = ol.Text;
            Type = ol.Type.ToString();
            CreationDate = ol.CreationDate;
        }

        public string Application { get; set; }
        public string Project { get; set; }
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public DateTime CreationDate { get; set; }
    }
}