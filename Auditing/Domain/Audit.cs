using System;

namespace Auditing.Domain
{
    public class Audit
    {
        //used for EF
        private Audit() {}

        public Audit(string application, string environment, string user, string entityId, string entityName, string entity, AuditAction action)
        {
            if (string.IsNullOrEmpty(application)) throw new ArgumentNullException("'Application'. Field can not be null or empty");
            Application = application;

            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException("'Environment'. Field can not be null or empty");
            Environment = environment;

            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("'User'. Field can not be null or empty");
            User = user;

            if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("'Id'. Field can not be null or empty");
            EntityId = entityId;

            if (string.IsNullOrEmpty(entityName)) throw new ArgumentNullException("'EntityName'. Field can not be null or empty");
            EntityName = entityName;

            if (action != AuditAction.Delete && string.IsNullOrEmpty(entity)) throw new ArgumentNullException("'Entity'. Field can not be null or empty when action is Create or Update");
            Entity = entity;

            if (action != AuditAction.Create && action != AuditAction.Update && action != AuditAction.Delete)
                throw new ArgumentOutOfRangeException("'Action'. Valid codes are: Create = 1, Update = 2, Delete = 3");
            Action = action;

            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Application { get; private set; }
        public string Environment { get; private set; }
        public string User { get; private set; }
        public string EntityId { get; private set; }
        public string Entity { get; private set; }
        public string Changes { get; private set; }
        public string EntityName { get; private set; }
        public AuditAction Action { get; private set; }
        public DateTime CreationDate { get; private set; }

        public void SetChanges(string changes)
        {
            if (string.IsNullOrEmpty(changes)) throw new ArgumentNullException($"Property 'Changes' on Entity.Name={EntityName} can not be null or empty");
            Changes = changes;
        }

        public void CheckIfCanBeAudited(Audit previousAudit)
        {
            if (previousAudit == null  && (Action == AuditAction.Update || Action == AuditAction.Delete))
                throw new InvalidOperationException("Missing previous data. Unable to calculate changes. Only 'Create' action is allowed");

            if (previousAudit != null)
            {
                if (previousAudit.EntityDeleted())
                    throw new InvalidOperationException($"Entity with Id={EntityId} has already been marked as 'Delete'. No actions are allowed");

                if (Action == AuditAction.Create)
                    throw new InvalidOperationException(
                        $"An Entity '{EntityName}' with Id={EntityId} has already been audited with 'Create' action, only 'Update/Delete' actions are allowed"
                    );
            }
        }

        public bool EntityDeleted() => Action == AuditAction.Delete;
    }
}