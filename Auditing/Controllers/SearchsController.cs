using System;
using System.Linq;
using System.Threading.Tasks;
using Auditing.Domain;
using Auditing.Dtos;
using Auditing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Infrastructure.CrossCuttingV3.Authentication;
using Shared.Infrastructure.CrossCuttingV3.Logging;

namespace Auditing.Controllers
{
    public class SearchsController : AuditingController
    {
        public SearchsController(
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
        public async Task<IActionResult> SearchAsync([FromBody] AuditSearchRequestDto auditSearchRequestDto)
        {
            return await ExecuteAsync(auditSearchRequestDto.Credential, async () =>
            {
                var page = auditSearchRequestDto.Page.Value;
                var pageSize = auditSearchRequestDto.PageSize.Value;
                var sortBy = auditSearchRequestDto.SortBy;
                var sortOrder = auditSearchRequestDto.SortOrder;
                var sinceDate = auditSearchRequestDto.SinceDate;
                var toDate = auditSearchRequestDto.ToDate;

                _logService.LogInfoMessageAsync($"AuditController.Get | INPUT | page={page} - pageSize={pageSize} - sortBy={sortBy} - sortOrder={sortOrder} - sinceDate={sinceDate} - toDate={toDate}");

                if (sortOrder != "asc" && sortOrder != "desc") throw new ArgumentOutOfRangeException($"Invalid sortOrder = {sortBy} parameter. Only 'asc' and 'desc' are allowed");

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
                _logService.LogInfoMessageAsync($"AuditController.Get | Searching done");

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
                _logService.LogInfoMessageAsync($"AuditController.Get | Sorting done");

                var pagedAudits = await sortedAudits.Skip(page * pageSize).Take(pageSize).ToListAsync();
                _logService.LogInfoMessageAsync($"AuditController.Get | Paging done");

                return Ok(pagedAudits.Select(pa => new AuditSearchResponseDto
                {
                    User = pa.User,
                    Changes = JsonConvert.DeserializeObject<dynamic>(pa.Changes),
                    Action = pa.Action,
                    CreationDate = pa.CreationDate
                }));
            });
        }


    }
}
