using Auditing.Domain;

namespace Auditing.Dtos
{
    public class AuditDtoPost
    {
        public string Application { get; set; }
        public string User { get; set; }
        public string Entity { get; set; }
        public string OldEntity { get; set; }
        public string EntityName { get; set; }
        public AuditAction Action { get; set; }
    }
}