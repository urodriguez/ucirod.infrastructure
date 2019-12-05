using System;
using Auditing.Domain.Enums;

namespace Auditing.Dtos
{
    public class AuditDto
    {
        public string Application { get; set; }
        public string User { get; set; }
        public string Entity { get; set; }
        public string OldEntity { get; set; }
        public AuditAction Action { get; set; }
        public DateTime Date { get; set; }

        public bool IsCreate()
        {
            return Action == AuditAction.Create;
        }
    }
}