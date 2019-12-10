using System.Collections.Generic;

namespace Auditing.Domain
{
    public interface IChangesService
    {
        IEnumerable<EntityChange> GetChanges(string serializedEntity, string serializedOldEntity, AuditAction auditAction);
    }
}