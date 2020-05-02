using System.Collections.Generic;
using System.Linq;
using Auditing.Domain;
using Auditing.Dtos;
using Auditing.Infrastructure.CrossCutting;
using Auditing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
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
                    entityChanges.AddRange(GetEntityChanges(entityJsonObject, AuditAction.Create));

                    break;

                case AuditAction.Update:
                    var oldEntityJsonObject = _jsonService.ExtractJsonJObject(serializedOldEntity);
                    var diffsJsonObject = (JObject) _jsonService.GetDifferences(entityJsonObject, oldEntityJsonObject);
                    if (diffsJsonObject == null) return null; //no changes on AuditAction = Update
                    entityChanges.AddRange(GetEntityChanges(diffsJsonObject, AuditAction.Update));

                    break;
            }

            var changesJsonObject = new JObject();
            SetChangesJsonArray(changesJsonObject, entityChanges);

            return changesJsonObject;
        }

        private static IEnumerable<EntityChange> GetEntityChanges(JObject jsonObject, AuditAction action, string entityPath = "")
        {
            var entityChanges = new List<EntityChange>();

            foreach (var jop in jsonObject.Properties())
            {
                switch (jop.Value.Type)
                {
                    case JTokenType.Object: //recursive call
                    {
                        var nestedObject = JsonConvert.DeserializeObject<JObject>(jop.Value.ToString());
                        var nestedEntityPath = entityPath == "" 
                            ? $"{jop.Path.Split(".").Last()}." 
                            : $"{entityPath}{jop.Path.Split(".").Last()}.";
                        entityChanges.AddRange(GetEntityChanges(nestedObject, action, nestedEntityPath));

                        break;
                    }                    
                    
                    case JTokenType.Array:
                    {
                        switch (action)
                        {
                            case AuditAction.Create:
                                foreach (var arrayItemJsonObject in (JArray)jop.Value)
                                {
                                    var nestedEntityPath = entityPath == "" 
                                        ? $"{arrayItemJsonObject.Path.Split(".").Last()}." 
                                        : $"{entityPath}{arrayItemJsonObject.Path.Split(".").Last()}.";
                                    entityChanges.AddRange(GetEntityChanges((JObject)arrayItemJsonObject, AuditAction.Create, nestedEntityPath));
                                }
                                break;

                            case AuditAction.Update:
                                entityChanges.Add(new EntityChange
                                {
                                    Field = entityPath == "" ? jop.Path : $"{entityPath}{jop.Path}",
                                    OldValue = JsonConvert.DeserializeObject<List<string>>(jop.Value.ToString()).Last(),
                                    NewValue = JsonConvert.DeserializeObject<List<string>>(jop.Value.ToString()).First()
                                });

                                break;
                        }

                        break;
                    }

                    default: //create case
                    {
                        if (jop.Path.Equals("_t") && jop.Value.ToString().Equals("a")) continue;//ignore internal field from JsonDiffPatch framework

                        string field;
                        if (jop.Path.Contains("[") && jop.Path.Contains("]."))
                            field = $"{entityPath}{jop.Path.Split(".").Last()}";
                        else
                            field = entityPath == "" ? jop.Path : $"{entityPath}{jop.Path}";

                        entityChanges.Add(new EntityChange
                        {
                            Field = field,
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

            var entityChangeProperties = entityChanges.GetPropertyNamesIgnoringDuplicates();
            foreach(var ecp in entityChangeProperties) {
                if (ecp.Type == EntityChangePropertyType.Plain)
                {
                    var entityChange = entityChanges.Where(ec => !ec.Field.Contains(".")).First(ec => ec.Field.Equals(ecp.Name));
                    ((JArray)changesJsonObject["Changes"]).Add(JObject.FromObject(entityChange));
                }
                else
                {
                    var nestedObjectJson = new JObject
                    {
                        ["Field"] = $"{ecp.Name}",
                        ["Changes"] = new JArray()
                    };

                    var nestedEntityChanges = entityChanges.Where(
                        ec => ecp.Type == EntityChangePropertyType.NestedObject 
                            ? ec.Field.Contains($"{ecp.Name}.") 
                            : ec.Field.Split(".").First().Contains($"{ecp.Name}[")
                    ).Select(
                        ec =>
                        {
                            string field;
                            if (ecp.Type == EntityChangePropertyType.NestedObject)
                            {
                                field = string.Join(".", ec.Field.Split(".").Skip(1));
                            }
                            else
                            {
                                var baseField = ec.Field.Split(".").First().Replace($"{ecp.Name}[", "").Replace("]", "");
                                field = baseField + "." + string.Join(".", ec.Field.Split(".").Skip(1));
                            }

                            return new EntityChange
                            {
                                Field = field,
                                OldValue = ec.OldValue,
                                NewValue = ec.NewValue
                            };
                        }
                    );

                    SetChangesJsonArray(nestedObjectJson, nestedEntityChanges);
                    ((JArray)changesJsonObject["Changes"]).Add(JObject.FromObject(nestedObjectJson));
                }
            }
        }
    }
}
