using System;

namespace Auditing.Domain
{
    public class Audit
    {
        //used for EF
        private Audit() {}

        public Audit(string application, string environment, string user, string entity, string entityName, AuditAction action)
        {
            SetApplication(application);
            SetEnvironment(environment);
            SetUser(user);
            SetEntity(entity);
            SetEntityName(entityName);
            SetAction(action);

            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Application { get; private set; }
        public string Environment { get; private set; }
        public string User { get; private set; }
        public string Entity { get; private set; }
        public string Changes { get; private set; }
        public string EntityId { get; private set; }
        public string EntityName { get; private set; }
        public AuditAction Action { get; private set; }
        public DateTime CreationDate { get; private set; }

        public void SetApplication(string application)
        {
            if (string.IsNullOrEmpty(application)) throw new ArgumentNullException("Application field can not be null or empty");
            Application = application;
        }

        public void SetEnvironment(string environment)
        {
            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException("Environment field can not be null or empty");
            Environment = environment;
        }

        public void SetAction(AuditAction action)
        {
            if (action != AuditAction.Create && action != AuditAction.Update && action != AuditAction.Delete) 
                throw new ArgumentOutOfRangeException("Invalid AuditAction code. Valid codes are: Create = 1, Update = 2, Delete = 3");
            Action = action;
        }

        public void SetUser(string user)
        {
            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("User field can not be null or empty");
            User = user;
        }

        public void SetEntity(string entity)
        {
            if (string.IsNullOrEmpty(entity)) throw new ArgumentNullException("Entity can not be null or empty");
            Entity = entity;
        }

        public void SetEntityId(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException($"Property 'Id' on Entity.Name={EntityName} is null or empty");
            EntityId = entityId;
        }

        public void SetEntityName(string entityName)
        {
            if (string.IsNullOrEmpty(entityName)) throw new ArgumentNullException("EntityName field can not be null or empty");
            EntityName = entityName;
        }

        public void SetChanges(string changes)
        {
            if (string.IsNullOrEmpty(changes)) throw new ArgumentNullException($"Property 'Changes' on Entity.Name={EntityName} can not be null or empty");
            Changes = changes;
        }

        public static void ValidateForAction(Audit previousAudit, AuditAction auditAction)
        {
            if ((auditAction == AuditAction.Update || auditAction == AuditAction.Delete) && previousAudit == null)
                throw new ArgumentNullException("None previous data found. History data is stored in order to calculate changes on 'Update/Delete' action");

            if (auditAction == AuditAction.Create && previousAudit != null)
                throw new InvalidOperationException("Entity has already been audited for 'Create' action. Only 'Update/Delete' actions are allowed");
        }

        public void ClearEntityForDelete()
        {
            if (Action == AuditAction.Delete) Entity = null;
        }
    }
}