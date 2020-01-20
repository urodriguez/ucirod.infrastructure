using System;
using System.Collections.Generic;
using System.Linq;
using Auditing.Domain;
using Auditing.Dtos;
using Auditing.Infrastructure.Persistence;
using Infrastructure.CrossCutting.LogService;
using Microsoft.AspNetCore.Mvc;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditsController : ControllerBase
    {
        private readonly AuditingDbContext _auditingDbContext;
        private readonly ILogService _logService;

        public AuditsController(AuditingDbContext auditingDbContext, ILogService logService)
        {
            _auditingDbContext = auditingDbContext;
            _logService = logService;
        }

        [HttpGet]
        public IActionResult Get(int page = 0, int pageSize = 10, string sortBy = "creationDate", string sortOrder = "desc", DateTime? sinceDate = null, DateTime? toDate = null)
        {
            _logService.LogInfoMessage($"AuditController.Get | INPUT | page={page} - pageSize={pageSize} - sortBy={sortBy} - sortOrder={sortOrder} - sinceDate={sinceDate} - toDate={toDate}");

            if (sortOrder != "asc" && sortOrder != "desc") return BadRequest($"Invalid sortOrder = {sortBy} parameter. Only 'asc' and 'desc' are allowed");

            IQueryable<Audit> searchedAudits;
            if (sinceDate == null && toDate == null)
            {
                sinceDate = DateTime.UtcNow.AddDays(-30);
                searchedAudits = _auditingDbContext.Audits.Where(sl => sinceDate <= sl.CreationDate);
            }
            else if (sinceDate != null && toDate != null)
            {
                var toDateAtEnd = toDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                searchedAudits = _auditingDbContext.Audits.Where(sl => sinceDate <= sl.CreationDate && sl.CreationDate <= toDateAtEnd);
            }
            else
            {
                var toDateAtEnd = toDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                searchedAudits = _auditingDbContext.Audits.Where(pl => sinceDate == null && toDate != null ? pl.CreationDate <= toDateAtEnd : sinceDate <= pl.CreationDate);
            }
            _logService.LogInfoMessage($"AuditController.Get | Searching done");

            IQueryable<Audit> sortedAudits;
            switch (sortBy)
            {
                case "application":
                    sortedAudits = sortOrder == "asc" ? searchedAudits.OrderBy(pl => pl.Application) : searchedAudits.OrderByDescending(pl => pl.Application);
                    break;
                case "user":
                    sortedAudits = sortOrder == "asc" ? searchedAudits.OrderBy(pl => pl.User) : searchedAudits.OrderByDescending(pl => pl.User);
                    break;
                case "action":
                    sortedAudits = sortOrder == "asc" ? searchedAudits.OrderBy(pl => pl.Action) : searchedAudits.OrderByDescending(pl => pl.Action);
                    break;
                case "creationDate":
                    sortedAudits = sortOrder == "asc" ? searchedAudits.OrderBy(pl => pl.CreationDate) : searchedAudits.OrderByDescending(pl => pl.CreationDate);
                    break;

                default:
                    return BadRequest($"Invalid sortBy = {sortBy} parameter");
            }
            _logService.LogInfoMessage($"AuditController.Get | Sorting done");

            var pagedAudits = sortedAudits.Skip(page * pageSize).Take(pageSize);
            _logService.LogInfoMessage($"AuditController.Get | Paging done");

            return Ok(pagedAudits.Select(pa => new AuditDtoGet
            {
                User = pa.User,
                Changes = JsonConvert.DeserializeObject<IEnumerable<EntityChange>>(pa.Changes),
                Action = pa.Action,
                CreationDate = pa.CreationDate
            }).ToList());
        }

        [HttpPost]
        public IActionResult Audit([FromBody] AuditDtoPost auditDto)
        {
            try
            {
                _logService.LogInfoMessage($"AuditController.Audit | INPUT | entity={auditDto.EntityName} - application={auditDto.Application} - user={auditDto.User} - action={auditDto.Action}");

                var audit = new Audit(auditDto.Application, auditDto.Environment, auditDto.User, auditDto.Entity, auditDto.EntityName, auditDto.Action);

                JObject entityObject;
                try
                {
                    entityObject = JObject.Parse(auditDto.Entity);
                }
                catch (Exception e)
                {
                    throw new Exception($"An error has occurred trying to parse Entity.Name={auditDto.EntityName}. FullStack trace is: {e.Message}");
                }

                var idProperty = entityObject.Properties().FirstOrDefault(p => p.Name == "Id");

                if (idProperty == null) throw new Exception($"Entity.Name={auditDto.EntityName} requires an \"Id\" property to be audited");

                audit.SetEntityId(idProperty.Value.ToString());

                var previousAudit = _auditingDbContext.Audits.Where(
                    a => a.EntityId == audit.EntityId && a.EntityName == audit.EntityName && a.Environment == audit.Environment
                ).OrderByDescending(
                    a => a.CreationDate
                ).FirstOrDefault();

                Domain.Audit.ValidateForAction(previousAudit, auditDto.Action);

                var entityChanges = GetChanges(auditDto.Entity, previousAudit.Entity, auditDto.Action);

                if (entityChanges.Any()) {
                    audit.Changes = JsonConvert.SerializeObject(entityChanges, Formatting.Indented);

                    _auditingDbContext.Audits.Add(audit);
                    _auditingDbContext.SaveChanges();

                    _logService.LogInfoMessage($"AuditController.Audit | Audit registry saved | audit.Id={audit.Id} - audit.EntityId={audit.EntityId}");
                } else
                {
                    _logService.LogInfoMessage($"AuditController.Audit | No changes were detected | audit.EntityId={audit.EntityId}");
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
