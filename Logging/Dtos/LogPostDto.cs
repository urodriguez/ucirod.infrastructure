﻿using System;
using Logging.Domain;

namespace Logging.Dtos
{
    public class LogPostDto
    {
        public string Application { get; set; }
        public string Project { get; set; }
        public Guid CorrelationId { get; set; }
        public string Text { get; set; }
        public LogType Type { get; set; }
    }
}