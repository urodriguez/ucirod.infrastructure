using System;
using System.Collections.Generic;
using System.Linq;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditing.Domain
{
    public class ChangesService : IChangesService
    {
        public IEnumerable<EntityChange> GetChanges(string serializedEntity, string serializedOldEntity, AuditAction auditAction)
        {
            JObject entityObject;
            try
            {
                entityObject = JObject.Parse(serializedEntity);
            }
            catch (Exception e)
            {
                throw new Exception($"An error has occurred trying to parse Entity. FullStack trace is: {e.Message}");
            }

            switch (auditAction)
            {
                case AuditAction.Create:
                    return entityObject.Properties().Select(ep => new EntityChange
                    {
                        Field = ep.Path,
                        OldValue = "",
                        NewValue = ep.Value.ToString()
                    });

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
                    var diffsObject = (JObject)jdp.Diff(entityObject, oldEntityObject);

                    if (diffsObject == null) return new List<EntityChange>();//no changes on AuditAction = Update

                    return diffsObject.Properties().Select(ep => new EntityChange
                    {
                        Field = ep.Path,
                        OldValue = JsonConvert.DeserializeObject<List<string>>(ep.Value.ToString()).Last(),
                        NewValue = JsonConvert.DeserializeObject<List<string>>(ep.Value.ToString()).First()
                    });

                case AuditAction.Delete:
                    return entityObject.Properties().Select(ep => new EntityChange
                    {
                        Field = ep.Path,
                        OldValue = ep.Value.ToString(),
                        NewValue = ""
                    });

                default:
                    return new List<EntityChange>();
            }
        }
    }
}