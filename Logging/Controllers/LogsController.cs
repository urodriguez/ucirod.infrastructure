using System;
using System.Linq;
using Logging.Domain;
using Logging.Dtos;
using Logging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Logging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly LoggingDbContext _loggingDbContext;

        public LogsController(LoggingDbContext loggingDbContext)
        {
            _loggingDbContext = loggingDbContext;
        }

        [HttpGet]
        public IActionResult Get(int page = 0, int pageSize = 10, string sortBy = "creationDate", string sortOrder = "desc", string searchWord = "", SearchLogType type = SearchLogType.Any, DateTime? sinceDate = null, DateTime? toDate = null)
        {
            if (sortOrder != "asc" && sortOrder != "desc") return BadRequest($"Invalid sortOrder = {sortBy} parameter. Only 'asc' and 'desc' are allowed");

            if (!Enum.IsDefined(typeof(SearchLogType), type))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(SearchLogType)).Select(slt => slt));
                throw new Exception($"Invalid Log Type on Log request object. Valid types are = {validTypes}");
            }

            var searchedLogs = searchWord == "" ? _loggingDbContext.Logs : _loggingDbContext.Logs.Where(
                pl => (type == SearchLogType.Any ? pl.Type == LogType.Trace || pl.Type == LogType.Info || pl.Type == LogType.Error : pl.Type == (LogType) type) &&
                      pl.Application.Equals(searchWord, StringComparison.InvariantCultureIgnoreCase) || 
                      pl.Project.Equals(searchWord, StringComparison.InvariantCultureIgnoreCase) || 
                      String.Equals(pl.CorrelationId.ToString(), searchWord, StringComparison.CurrentCultureIgnoreCase) || 
                      pl.Text.Contains(searchWord, StringComparison.InvariantCultureIgnoreCase)
            );

            if (sinceDate == null && toDate == null)
            {
                sinceDate = DateTime.UtcNow.AddDays(-30);
                searchedLogs = searchedLogs.Where(sl => sinceDate <= sl.CreationDate);
            }
            else if (sinceDate != null && toDate != null)
            {
                var toDateAtEnd = toDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                searchedLogs = searchedLogs.Where(sl => sinceDate <= sl.CreationDate && sl.CreationDate <= toDateAtEnd);
            }
            else
            {
                var toDateAtEnd = toDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                searchedLogs = searchedLogs.Where(pl => sinceDate == null && toDate != null ? pl.CreationDate <= toDateAtEnd : sinceDate <= pl.CreationDate);
            }

            IQueryable<Log> orderedLogs;
            switch (sortBy)
            {
                case "application":
                    orderedLogs = sortOrder == "asc" ? searchedLogs.OrderBy(pl => pl.Application) : searchedLogs.OrderByDescending(pl => pl.Application);
                    break;
                case "project":
                    orderedLogs = sortOrder == "asc" ? searchedLogs.OrderBy(pl => pl.Project) : searchedLogs.OrderByDescending(pl => pl.Project);
                    break;
                case "type":
                    orderedLogs = sortOrder == "asc" ? searchedLogs.OrderBy(pl => pl.Type) : searchedLogs.OrderByDescending(pl => pl.Type);
                    break;
                case "creationDate":
                    orderedLogs = sortOrder == "asc" ? searchedLogs.OrderBy(pl => pl.CreationDate) : searchedLogs.OrderByDescending(pl => pl.CreationDate);
                    break;

                default:
                    return BadRequest($"Invalid sortBy = {sortBy} parameter");
            }

            var pagedLogs = orderedLogs.Skip(page * pageSize).Take(pageSize);

            return Ok(pagedLogs.Select(ol => new LogDtoGet(ol)).ToList());
        }

        [HttpPost]
        public IActionResult Post([FromBody] LogDtoPost logDto)
        {
            try
            {
                var log = new Log(logDto.Application, logDto.Project, logDto.CorrelationId, logDto.Text, logDto.Type);

                _loggingDbContext.Logs.Add(log);
                _loggingDbContext.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}
