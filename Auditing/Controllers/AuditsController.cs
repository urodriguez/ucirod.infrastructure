using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auditing.Domain.Enums;
using Auditing.Dtos;
using Auditing.Infrastructure.CrossCutting.Logging;
using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditsController : ControllerBase
    {
        private readonly ILogService _logService;

        public AuditsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            //_logService.QueueInfoMessage($"AuditController - Getting entities audited");
            //_logService.FlushQueueMessages();
            return new string[] { "audit1", "audit2" };
        }

        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "audit" + id;
        }

        [HttpPost]
        public void Audit([FromBody] AuditDto auditDto)
        {
            //_logService.QueueInfoMessage($"AuditController - Auditing entity with id = {auditDto.EntityId}");
            //_logService.FlushQueueMessages();

            var objectChanges = GetChanges(auditDto.Entity, auditDto.OldEntity, auditDto.Action);
        }

        public class EntityChange
        {
            public string Field { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
        }

        public IEnumerable<EntityChange> GetChanges(string serializedEntity, string serializedOldEntity, AuditAction auditAction)
        {
            if (serializedEntity == null) throw new Exception("Entity can not be null");

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
                    if (serializedOldEntity == null) throw new Exception("OldEntity can not be null on Update action");

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
                    throw new Exception("Invalid AuditAction code. Valid codes are: Create = 0, Update = 1, Delete = 2");
            }
        }
    }
}
