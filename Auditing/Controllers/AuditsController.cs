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

                var entityChanges = GetEntityChangesJson(audit.Entity, previousAudit?.Entity, audit.Action);

                if (entityChanges == null)
                {
                    _logService.LogInfoMessage($"AuditController.Audit | No changes were detected | audit.EntityId={audit.EntityId}");
                    return Ok();
                }

                audit.SetChanges(entityChanges.ToString());

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

                case AuditAction.Delete:
                    entityChanges.AddRange(GetEntityChanges(entityJsonObject, AuditAction.Delete));
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
                    case JTokenType.Object: //object types are processing in the same way for Create, Update, Delete
                    {
                        var nestedObject = (JObject)jop.Value;
                        var nestedEntityPath = entityPath == "" 
                            ? $"{jop.Path.Split(".").Last()}." 
                            : $"{entityPath}{jop.Path.Split(".").Last()}.";
                        entityChanges.AddRange(GetEntityChanges(nestedObject, action, nestedEntityPath));

                        break;
                    }                    
                    
                    case JTokenType.Array: //array types are processing differently based on Create, Update, Delete
                        {
                        switch (action)
                        {
                            case AuditAction.Create:
                                foreach (var arrayItemJsonObject in (JArray)jop.Value)
                                {
                                    var nestedEntityPath = entityPath == "" 
                                        ? $"{arrayItemJsonObject.Path.Split(".").Last()}." 
                                        : $"{entityPath}{arrayItemJsonObject.Path.Split(".").Last()}.";
                                    nestedEntityPath = nestedEntityPath.Replace("[", ".").Replace("]", "");
                                    entityChanges.AddRange(GetEntityChanges((JObject)arrayItemJsonObject, AuditAction.Create, nestedEntityPath));
                                }

                                break;

                            case AuditAction.Update: //updates are received as json array
                                if (jop.Path.Contains("_")) //when new object is added on existing array
                                {
                                    var nestedEntityPath = entityPath == ""
                                        ? $"{jop.Path.Replace("_", "").Split(".").Last()}."
                                        : $"{entityPath}{jop.Path.Replace("_", "").Split(".").Last()}.";
                                    entityChanges.AddRange(GetEntityChanges((JObject)jop.Value.First, AuditAction.Create, nestedEntityPath));
                                }
                                else
                                {
                                    if (((JArray)jop.Value).Count == 1) //when prop is removed from existing object
                                    {
                                        switch (jop.Value.First.Type)
                                        {
                                            case JTokenType.Object:
                                            {
                                                var nestedEntityPath = entityPath == ""
                                                    ? $"{jop.Path.Split(".").Last()}."
                                                    : $"{entityPath}{jop.Path.Split(".").Last()}.";
                                                entityChanges.AddRange(GetEntityChanges((JObject)jop.Value.First, AuditAction.Delete, nestedEntityPath));
                                                break;
                                            }

                                            case JTokenType.Array:
                                            {
                                                foreach (var arrayItemJsonObject in (JArray)jop.Value.First)
                                                {
                                                    var nestedArrayEntityPath = entityPath == ""
                                                        ? $"{arrayItemJsonObject.Path.Split(".").Last()}."
                                                        : $"{entityPath}{arrayItemJsonObject.Path.Split(".").Last()}.";
                                                    var nestedArrayEntityPathElements = nestedArrayEntityPath.Replace("[", ".").Replace("]", "").Split(".").ToList();
                                                    nestedArrayEntityPathElements.RemoveAt(1);
                                                    nestedArrayEntityPath = string.Join(
                                                        ".",
                                                        nestedArrayEntityPathElements
                                                    );
                                                    entityChanges.AddRange(GetEntityChanges((JObject)arrayItemJsonObject, AuditAction.Delete, nestedArrayEntityPath));
                                                }
                                                break;
                                            }

                                            default:
                                            {
                                                entityChanges.Add(new EntityChange
                                                {
                                                    Field = entityPath == "" ? jop.Path.Split(".").Last() : $"{entityPath}{jop.Path.Split(".").Last()}",
                                                    OldValue = jop.Value.First.ToString(),
                                                    NewValue = "undefined"
                                                });
                                                break;
                                            }

                                        }
                                    }                                    
                                    
                                    if (((JArray)jop.Value).Count == 2) //when existing object is updated on existing object
                                    {
                                        entityChanges.Add(new EntityChange
                                        {
                                            Field = entityPath == "" ? jop.Path.Split(".").Last() : $"{entityPath}{jop.Path.Split(".").Last()}",
                                            OldValue = ((JArray)jop.Value).Last.ToString(),
                                            NewValue = ((JArray)jop.Value).First.ToString()
                                        });
                                    }

                                    if (((JArray)jop.Value).Count == 3) //when new prop is added on existing object
                                    {
                                        switch (jop.Value.First.Type)
                                        {
                                            case JTokenType.Object:
                                            {
                                                var nestedEntityPath = entityPath == ""
                                                    ? $"{jop.Path.Split(".").Last()}."
                                                    : $"{entityPath}{jop.Path.Split(".").Last()}.";
                                                entityChanges.AddRange(GetEntityChanges((JObject)jop.Value.First, AuditAction.Create, nestedEntityPath));
                                                break;
                                            }

                                            case JTokenType.Array:
                                            {
                                                foreach (var arrayItemJsonObject in (JArray)jop.Value.First)
                                                {
                                                    var nestedArrayEntityPath = entityPath == ""
                                                        ? $"{arrayItemJsonObject.Path.Split(".").Last()}."
                                                        : $"{entityPath}{arrayItemJsonObject.Path.Split(".").Last()}.";
                                                    var nestedArrayEntityPathElements = nestedArrayEntityPath.Replace("[", ".").Replace("]", "").Split(".").ToList();
                                                    nestedArrayEntityPathElements.RemoveAt(1);
                                                    nestedArrayEntityPath = string.Join(
                                                        ".",
                                                        nestedArrayEntityPathElements
                                                    );
                                                    entityChanges.AddRange(GetEntityChanges((JObject)arrayItemJsonObject, AuditAction.Create, nestedArrayEntityPath));
                                                }
                                                break;
                                            }

                                            default:
                                            {
                                                entityChanges.Add(new EntityChange
                                                {
                                                    Field = entityPath == "" ? jop.Path.Split(".").Last() : $"{entityPath}{jop.Path.Split(".").Last()}",
                                                    OldValue = "undefined",
                                                    NewValue = jop.Value.First.ToString()
                                                });
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                        }

                        break;
                    }

                    default: //Plain types (strings, numbers, boolean - no nested object/array) - plain types are processing in the same way for Create, Update, Delete
                        {
                        if (jop.Path.Contains("_t") && jop.Value.ToString().Equals("a")) continue;//ignore internal field from JsonDiffPatch framework

                        string field;
                        if (jop.Path.Contains("[") && jop.Path.Contains("]."))
                            field = $"{entityPath}{jop.Path.Split(".").Last()}";
                        else
                            field = entityPath == "" ? jop.Path : $"{entityPath}{jop.Path}";

                        entityChanges.Add(new EntityChange
                        {
                            Field = field,
                            OldValue = action == AuditAction.Create ? "undefined" : jop.Value.ToString(), //OldValue = "undefined" on Create
                            NewValue = action == AuditAction.Create ? jop.Value.ToString() : "undefined"  //NewValue = "undefined" on Delete
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
                            ? ec.Field.Split(".").First().Contains($"{ecp.Name}") 
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
