using System;
using System.Collections.Generic;
using System.Linq;
using Auditing.Domain;
using Auditing.Dtos;
using Auditing.Infrastructure.CrossCutting;
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
        private readonly IJsonService _jsonService;

        public AuditsController(
            AuditingDbContext auditingDbContext,
            ICredentialService credentialService,
            ILogService logService, 
            IJsonService jsonService
        ) : base(
            auditingDbContext,
            credentialService,
            logService
        )
        {
            _jsonService = jsonService;
        }

        [HttpPost]
        public IActionResult Audit([FromBody] AuditDtoPost auditDto)
        {
            return Execute(auditDto.Credential, () =>
            {
                var audit = new Audit(auditDto.Application, auditDto.Environment, auditDto.User, auditDto.EntityId, auditDto.EntityName, auditDto.Entity, auditDto.Action);

                _logService.LogInfoMessage($"AuditController.Audit | Audit entity ready | entity={audit.EntityName} - entityId={audit.EntityId} - application={audit.Application} - user={audit.User} - action={audit.Action}");

                var previousAudit = _auditingDbContext.Audits.Where(
                    a => a.EntityId == audit.EntityId && a.EntityName == audit.EntityName && a.Environment == audit.Environment && a.Application == audit.Application
                ).OrderByDescending(
                    a => a.CreationDate
                ).FirstOrDefault();

                audit.CheckIfCanBeAudited(previousAudit);

                if (!audit.EntityDeleted())
                {
                    var entityChanges = GetEntityChangesJson(audit.Entity, previousAudit?.Entity, audit.Action);

                    if (entityChanges == null)
                    {
                        _logService.LogInfoMessage($"AuditController.Audit | No changes were detected | audit.EntityId={audit.EntityId}");
                        return Ok();
                    }

                    audit.SetChanges(entityChanges.ToString());
                }

                _auditingDbContext.Audits.Add(audit);
                _auditingDbContext.SaveChanges();

                _logService.LogInfoMessage($"AuditController.Audit | Audit registry saved | audit.Id={audit.Id} - audit.EntityId={audit.EntityId}");

                return Ok();
            });
        }

        private JObject GetEntityChangesJson(string serializedEntity, string serializedOldEntity, AuditAction auditAction)
        {
            var entityJsonObject = _jsonService.ExtractJsonJObject(serializedEntity);

            var entityChanges = new List<EntityChange>();

            switch (auditAction)
            {
                case AuditAction.Create:
                    entityChanges.AddRange(GetEntityChanges(entityJsonObject));

                    break;

                case AuditAction.Update:
                    var oldEntityJsonObject = _jsonService.ExtractJsonJObject(serializedOldEntity);
                    var diffsJsonObject = (JObject) _jsonService.GetDifferences(entityJsonObject, oldEntityJsonObject);
                    if (diffsJsonObject == null) return null; //no changes on AuditAction = Update
                    entityChanges.AddRange(GetEntityChanges(diffsJsonObject));

                    break;
            }

            var changesJsonObject = new JObject();
            SetChangesJsonArray(changesJsonObject, entityChanges);

            return changesJsonObject;
        }

        private static IEnumerable<EntityChange> GetEntityChanges(JObject jsonObject, string entityPath = "")
        {
            var entityChanges = new List<EntityChange>();

            foreach (var jop in jsonObject.Properties())
            {
                switch (jop.Value.Type)
                {
                    case JTokenType.Object: //recursive call
                    {
                        var nestedObject = JsonConvert.DeserializeObject<JObject>(jop.Value.ToString());
                        var nestedEntityPath = entityPath == "" ? $"{jop.Path}." : $"{entityPath}{jop.Path}.";
                        entityChanges.AddRange(GetEntityChanges(nestedObject, nestedEntityPath));

                        break;
                    }                    
                    
                    case JTokenType.Array: //update case
                    {
                        entityChanges.Add(new EntityChange
                        {
                            Field = entityPath == "" ? jop.Path : $"{entityPath}{jop.Path}",
                            OldValue = JsonConvert.DeserializeObject<List<string>>(jop.Value.ToString()).Last(),
                            NewValue = JsonConvert.DeserializeObject<List<string>>(jop.Value.ToString()).First()
                        });

                        break;
                    }

                    default: //create case
                    {
                        entityChanges.Add(new EntityChange
                        {
                            Field = entityPath == "" ? jop.Path : $"{entityPath}{jop.Path}",
                            OldValue = "undefined",
                            NewValue = jop.Value.ToString()
                        });

                        break;
                    }
                }
            }

            return entityChanges;
        }

        private static void SetChangesJsonArray(JObject changesJsonObject, IEnumerable<EntityChange> entityChanges)
        {
            changesJsonObject["Changes"] = new JArray();

            var props = entityChanges.GetNotDuplicatedPropertyNames();
            foreach(var p in props) {
                if (entityChanges.IsNestedObject(p))
                {
                    var nestedObjectJson = new JObject
                    {
                        ["Field"] = $"{p}",
                        ["Changes"] = new JArray()
                    };

                    var nestedEntityChanges = entityChanges.Where(
                        ec => ec.Field.Contains(p)
                    ).Select(
                        ec => new EntityChange {Field = ec.Field.Replace($"{p}.", ""), OldValue = ec.OldValue, NewValue = ec.NewValue}
                    ).ToList();

                    SetChangesJsonArray(nestedObjectJson, nestedEntityChanges);
                    ((JArray)changesJsonObject["Changes"]).Add(JObject.FromObject(nestedObjectJson));
                }
                else
                {
                    var entityChange = entityChanges.Where(ec => !ec.Field.Contains(".")).First(ec => ec.Field.Equals(p));
                    ((JArray)changesJsonObject["Changes"]).Add(JObject.FromObject(entityChange));
                }
            }
        }
    }
}
