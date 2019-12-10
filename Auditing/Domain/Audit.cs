using System;

namespace Auditing.Domain
{
    public class Audit
    {
        public Audit(string application, string user, string entityName, AuditAction action)
        {
            if (string.IsNullOrEmpty(application)) throw new Exception("Application field can not be null or empty");
            if (string.IsNullOrEmpty(user)) throw new Exception("User field can not be null or empty");
            if (string.IsNullOrEmpty(entityName)) throw new Exception("EntityName field can not be null or empty");
            if(action != AuditAction.Create && action != AuditAction.Update && action != AuditAction.Delete) throw new Exception("Invalid AuditAction code. Valid codes are: Create = 0, Update = 1, Delete = 2");

            Id = Guid.NewGuid();
            Application = application;
            User = user;
            EntityName = entityName;
            Action = action;
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public string Application { get; set; }
        public string User { get; set; }
        public string Changes { get; set; }
        public string EntityName { get; set; }
        public AuditAction Action { get; set; }
        public DateTime CreationDate { get; set; }

        public void ValidateSerializedEntity(string serializedEntity)
        {
            if (serializedEntity == null) throw new Exception("Entity can not be null");
        }

        public void ValidateOldSerializedEntity(string serializedOldEntity, AuditAction action)
        {
            if (action == AuditAction.Update && serializedOldEntity == null) throw new Exception("OldEntity can not be null on Update action");
        }
    }
}