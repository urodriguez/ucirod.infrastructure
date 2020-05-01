using Auditing.Domain;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Auditing.Dtos
{
    public class AuditDtoPost
    {
        public Credential Credential { get; set; }
        public string Application { get; set; }
        public string Environment { get; set; }
        public string User { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string Entity { get; set; }
        public AuditAction Action { get; set; }
    }
}