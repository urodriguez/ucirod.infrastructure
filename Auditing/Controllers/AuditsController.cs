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
using Infrastructure.CrossCutting.Authentication;
using Logging.Application;
using Microsoft.Extensions.Configuration;

namespace Auditing.Controllers
{
    [Route("api/[controller]")]
    public class AuditsController : AuditingController
    {
        public AuditsController(
            AuditingDbContext auditingDbContext, 
            IClientService clientService, 
            ILogService logService, 
            ICorrelationService correlationService, 
            IConfiguration config
        ) : base(
            auditingDbContext,
            clientService,
            logService,
            correlationService,
            config
        )
        {
        }

        [HttpPost]
        public IActionResult Audit([FromBody] AuditDtoPost auditDto)
        {
            return Execute(auditDto.Account, () =>
            {
                var audit = new Audit(auditDto.Application, auditDto.Environment, auditDto.User, auditDto.Entity, auditDto.EntityName, auditDto.Action);

                JObject entityObject = ExtractEntityParsed(audit);

                var idProperty = entityObject.Properties().FirstOrDefault(p => p.Name == "Id");

                if (idProperty == null) throw new Exception($"Entity.Name={audit.EntityName} requires an \"Id\" property to be audited");

                audit.SetEntityId(idProperty.Value.ToString());

                _logService.LogInfoMessage($"AuditController.Audit | Audit entity ready | entity={audit.EntityName} - entityId={audit.EntityId} - application={audit.Application} - user={audit.User} - action={audit.Action}");

                var previousAudit = _auditingDbContext.Audits.Where(
                    a => a.EntityId == audit.EntityId && a.EntityName == audit.EntityName && a.Environment == audit.Environment
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

        private static JObject ExtractEntityParsed(Audit audit)
        {
            JObject entityObject;
            try
            {
                entityObject = JObject.Parse(audit.Entity);
            }
            catch (Exception e)
            {
                throw new Exception($"An error has occurred trying to parse Entity.Name={audit.EntityName}. FullStack trace is: {e.Message}");
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
                    catch (Exception e)
                    {
                        throw new Exception($"An error has occurred trying to parse OldEntity. FullStack trace is: {e.Message}");
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
