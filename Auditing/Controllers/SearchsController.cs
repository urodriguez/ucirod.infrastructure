using System;
using System.Collections.Generic;
using System.Linq;
using Auditing.Domain;
using Auditing.Dtos;
using Auditing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Infrastructure.CrossCutting.Authentication;
using System.Reflection;
using Logging.Application;
using Logging.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Auditing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchsController : ControllerBase
    {
        private readonly AuditingDbContext _auditingDbContext;
        private readonly ILogService _logService;
        private readonly IClientService _clientService;

        public SearchsController(AuditingDbContext auditingDbContext, IClientService clientService, ILogService logService, ICorrelationService correlationService, IConfiguration config)
        {
            _auditingDbContext = auditingDbContext;
            _clientService = clientService;

            _logService = logService;
            _logService.Configure(new LogSettings
            {
                Application = "Infrastructure",
                Project = "Auditing",
                Environment = config.GetValue<string>("Environment"),
                CorrelationId = correlationService.Create(null, false).Id
            });
        }

        [HttpPost]
        public IActionResult Search([FromBody] AuditSearchRequestDto auditSearchRequestDto)
        {
            try
            {
                var page = auditSearchRequestDto.Page.Value;
                var pageSize = auditSearchRequestDto.PageSize.Value;
                var sortBy = auditSearchRequestDto.SortBy;
                var sortOrder = auditSearchRequestDto.SortOrder;
                var sinceDate = auditSearchRequestDto.SinceDate;
                var toDate = auditSearchRequestDto.ToDate;

                _logService.LogInfoMessage($"AuditController.Get | INPUT | page={page} - pageSize={pageSize} - sortBy={sortBy} - sortOrder={sortOrder} - sinceDate={sinceDate} - toDate={toDate}");

                if (!_clientService.CredentialsAreValid(auditSearchRequestDto.Account))
                {
                    if (auditSearchRequestDto.Account == null)
                    {
                        _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | BadRequest | account == null");
                        throw new ArgumentNullException("Credentials not provided");
                    }
                    _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Unauthorized | account.Id={auditSearchRequestDto.Account.Id}");
                    throw new UnauthorizedAccessException();
                }
                _logService.LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={auditSearchRequestDto.Account.Id}");

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
                        throw new ArgumentOutOfRangeException($"Invalid sortBy = {sortBy} parameter");
                }
                _logService.LogInfoMessage($"AuditController.Get | Sorting done");

                var pagedAudits = sortedAudits.Skip(page * pageSize).Take(pageSize);
                _logService.LogInfoMessage($"AuditController.Get | Paging done");

                return Ok(pagedAudits.Select(pa => new AuditSearchResponseDto
                {
                    User = pa.User,
                    Changes = JsonConvert.DeserializeObject<IEnumerable<EntityChange>>(pa.Changes),
                    Action = pa.Action,
                    CreationDate = pa.CreationDate
                }).ToList());
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized();
            }
            catch (ArgumentNullException ane)
            {
                return BadRequest(ane.Message);
            }
            catch (ArgumentOutOfRangeException aore)
            {
                return BadRequest(aore.Message);
            }
            catch (Exception e)
            {
                _logService.LogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Exception | e.FullStackTrace={e}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
