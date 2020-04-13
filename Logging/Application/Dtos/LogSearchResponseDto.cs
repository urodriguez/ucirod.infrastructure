using System;
using Logging.Domain;

namespace Logging.Application.Dtos
{
    public class LogSearchResponseDto
    {
        public LogSearchResponseDto(Log l)
        {
            Application = l.Application;
            Project = l.Project;
            Text = l.Text;
            Type = l.Type.ToString();
            CreationDate = l.CreationDate;
        }

        public string Application { get; set; }
        public string Project { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public DateTime CreationDate { get; set; }
    }
}