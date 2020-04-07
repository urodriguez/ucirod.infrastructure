using System;
using System.Collections.Generic;
using System.Linq;
using Auditing.Domain;
using Auditing.Dtos;
using Auditing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Infrastructure.CrossCutting.Authentication;
using Shared.Infrastructure.CrossCutting.Logging;

namespace Auditing.Controllers
{
    public class AuditsController : AuditingController
    {
        public AuditsController(
            AuditingDbContext auditingDbContext, 
            ICredentialService credentialService, 
            ILogService logService
        ) : base(
            auditingDbContext,
            credentialService,
            logService
        )
        {
        }

        [HttpPost]
        public IActionResult Audit([FromBody] AuditDtoPost auditDto)
        {
            return Execute(auditDto.Credential, () =>
            {
                var audit = new Audit(auditDto.Application, auditDto.Environment, auditDto.User, auditDto.Entity, auditDto.EntityName, auditDto.Action);

                JObject entityObject = ExtractEntityObject(audit);

                var idProperty = entityObject.Properties().FirstOrDefault(p => p.Name == "Id");

                if (idProperty == null) throw new KeyNotFoundException($"Key/Property \"Id\" not found on Entity.Name={audit.EntityName}");

                audit.SetEntityId(idProperty.Value.ToString());

                _logService.LogInfoMessage($"AuditController.Audit | Audit entity ready | entity={audit.EntityName} - entityId={audit.EntityId} - application={audit.Application} - user={audit.User} - action={audit.Action}");

                var previousAudit = _auditingDbContext.Audits.Where(
                    a => a.EntityId == audit.EntityId && a.EntityName == audit.EntityName && a.Environment == audit.Environment && a.Application == audit.Application
                ).OrderByDescending(
                    a => a.CreationDate
                ).FirstOrDefault();

                Domain.Audit.ValidateForAction(previousAudit, audit.Action);

                var entityChanges = GetChanges(audit.Entity, previousAudit?.Entity, audit.Action);
                if (!entityChanges.Any())
                {
                    _logService.LogInfoMessage($"AuditController.Audit | No changes were detected | audit.EntityId={audit.EntityId}");
                    return Ok();
                }
                audit.SetChanges(JsonConvert.SerializeObject(entityChanges, Formatting.Indented));

                audit.ClearEntityForDelete();

                _auditingDbContext.Audits.Add(audit);
                _auditingDbContext.SaveChanges();

                _logService.LogInfoMessage($"AuditController.Audit | Audit registry saved | audit.Id={audit.Id} - audit.EntityId={audit.EntityId}");

                return Ok();
            });
        }

        private static JObject ExtractEntityObject(Audit audit)
        {
            JObject entityObject;
            try
            {
                entityObject = JObject.Parse(audit.Entity);
            }
            catch (Exception)
            {
                throw new FormatException($"An error has occurred trying to parse Entity.Name={audit.EntityName} - Entity.Id={audit.EntityId}. Check Json format");
            }

            return entityObject;
        }

        private static IEnumerable<EntityChange> GetChanges(string serializedEntity, string serializedOldEntity, AuditAction auditAction)
        {
            var entityObject = JObject.Parse(serializedEntity);

            IEnumerable<EntityChange> entityChanges = null;

            switch (auditAction)
            {
                case AuditAction.Create:
                    entityChanges = entityObject.Properties().Select(ep => new EntityChange
                    {
                        Field = ep.Path,
                        OldValue = "",
                        NewValue = ep.Value.ToString()
                    });
                    break;

                case AuditAction.Update:
                    JObject oldEntityObject;
                    try
                    {
                        oldEntityObject = JObject.Parse(serializedOldEntity);
                    }
                    catch (Exception)
                    {
                        throw new FormatException($"An error has occurred trying to parse OldEntity. Check Json format");
                    }

                    var jdp = new JsonDiffPatch();
                    var diffsObject = (JObject) jdp.Diff(entityObject, oldEntityObject);

                    if (diffsObject == null) return new List<EntityChange>(); //no changes on AuditAction = Update

                    entityChanges = diffsObject.Properties().Select(ep => new EntityChange
                    {
                        Field = ep.Path,
                        OldValue = JsonConvert.DeserializeObject<List<string>>(ep.Value.ToString()).Last(),
                        NewValue = JsonConvert.DeserializeObject<List<string>>(ep.Value.ToString()).First()
                    });
                    break;

                case AuditAction.Delete:
                    entityChanges = entityObject.Properties().Select(ep => new EntityChange
                    {
                        Field = ep.Path,
                        OldValue = ep.Value.ToString(),
                        NewValue = ""
                    });
                    break;
            }

            return entityChanges;
        }
    }
}
