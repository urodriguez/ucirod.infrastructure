using System;
using System.Text.RegularExpressions;

namespace Auditing.Domain
{
    public class Audit
    {
        public Audit(string application, string environment, string user, string entity, string entityName, AuditAction action)
        {
            if (string.IsNullOrEmpty(application)) throw new Exception("Application field can not be null or empty");
            if (string.IsNullOrEmpty(environment)) throw new Exception("Environment field can not be null or empty");
            if (string.IsNullOrEmpty(user)) throw new Exception("User field can not be null or empty");
            if (string.IsNullOrEmpty(entity)) throw new Exception("Entity can not be null or empty");
            if (string.IsNullOrEmpty(entityName)) throw new Exception("EntityName field can not be null or empty");
            if(action != AuditAction.Create && action != AuditAction.Update && action != AuditAction.Delete) throw new Exception("Invalid AuditAction code. Valid codes are: Create = 1, Update = 2, Delete = 3");

            Id = Guid.NewGuid();
            Application = application;
            Environment = environment;
            User = user;
            Entity = entity;
            EntityName = entityName;
            Action = action;
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public string Application { get; set; }
        public string Environment { get; set; }
        public string User { get; set; }
        public string Entity { get; set; }
        public string Changes { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public AuditAction Action { get; set; }
        public DateTime CreationDate { get; set; }

        public void SetEntityId(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) throw new Exception($"Property \"Id\" on Entity.Name={EntityName} is null or empty");
        }

        public static void ValidateForAction(Audit previousAudit, AuditAction auditAction)
        {
            if ((auditAction == AuditAction.Update || auditAction == AuditAction.Delete) && previousAudit == null)
                throw new Exception("None previous data found. History data is stored in order to calculate changes on Update/Delete action");
        }
    }
}