using System;
using Auditing.Domain;

namespace Auditing.Dtos
{
    public class AuditSearchResponseDto
    {
        public string User { get; set; }
        public dynamic Changes { get; set; }
        public AuditAction Action { get; set; }
        public DateTime CreationDate { get; set; }
    }
}