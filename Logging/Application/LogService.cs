using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Infrastructure.CrossCutting.Authentication;
using Logging.Application.Dtos;
using Logging.Domain;
using Logging.Infrastructure.Persistence;

namespace Logging.Application
{
    public class LogService : ILogService
    {
        private readonly LoggingDbContext _loggingDbContext;
        private readonly IClientService _clientService;

        private LogSettings _logSettings;

        public LogService(LoggingDbContext loggingDbContext, IClientService clientService)
        {
            _loggingDbContext = loggingDbContext;
            _clientService = clientService;
        }

        public void Configure(LogSettings logSettings)
        {
            _logSettings = logSettings;
        }

        public void Log(LogDtoPost logDto, bool validateCredentials = true)
        {
            if (validateCredentials)
            {
                if (!_clientService.CredentialsAreValid(logDto.Account)) 
                {
                    if (logDto.Account == null) throw new ArgumentNullException("Credentials not provided");
                    throw new UnauthorizedAccessException();
                }
                LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={logDto.Account.Id}");
            }

            var log = new Log(logDto.Application, logDto.Project, logDto.CorrelationId, logDto.Text, logDto.Type, logDto.Environment);

            _loggingDbContext.Logs.Add(log);
            _loggingDbContext.SaveChanges();
        }

        public IEnumerable<LogDtoGet> Search(LogSearchRequestDto logSearchRequestDto)
        {
            if (!_clientService.CredentialsAreValid(logSearchRequestDto.Account))
            {
                if (logSearchRequestDto.Account == null) throw new ArgumentNullException("Credentials not provided");
                throw new UnauthorizedAccessException();
            }
            LogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}  | Authorized | account.Id={logSearchRequestDto.Account.Id}");

            var page = logSearchRequestDto.Page.Value;
            var pageSize = logSearchRequestDto.PageSize.Value;
            var sortBy = logSearchRequestDto.SortBy;
            var sortOrder = logSearchRequestDto.SortOrder;
            var searchWord = logSearchRequestDto.SearchWord;
            var logType = logSearchRequestDto.LogType;
            var environment = logSearchRequestDto.Environment;
            var sinceDate = logSearchRequestDto.SinceDate;
            var toDate = logSearchRequestDto.ToDate;

            if (sortOrder != "asc" && sortOrder != "desc") throw new ArgumentOutOfRangeException($"Invalid sortOrder = {sortBy} parameter. Only 'asc' and 'desc' are allowed");

            if (logType != null && !Enum.IsDefined(typeof(LogType), logType))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(LogType)).Select(lt => lt));
                throw new ArgumentOutOfRangeException($"Invalid Log Type on Log request object. Valid types are = {validTypes}");
            }

            Func<Log, bool> searchWordNotNullOrEmptyExp = log => log.Application.Equals(searchWord, StringComparison.InvariantCultureIgnoreCase) ||
                                                                 log.Project.Equals(searchWord, StringComparison.InvariantCultureIgnoreCase) ||
                                                                 String.Equals(log.CorrelationId.ToString(), searchWord, StringComparison.CurrentCultureIgnoreCase) ||
                                                                 log.Text.Contains(searchWord, StringComparison.InvariantCultureIgnoreCase);

            var searchWordExp = !string.IsNullOrEmpty(searchWord) ? searchWordNotNullOrEmptyExp : l => true;

            var searchedLogs = _loggingDbContext.Logs.Where(
                log =>(logType == null ? log.Type == LogType.Trace || log.Type == LogType.Info || log.Type == LogType.Error : log.Type == logType) &&

                    searchWordExp(log) &&

                    (string.IsNullOrEmpty(environment) ? log.Environment == "DEV" || log.Environment == "TEST" || log.Environment == "STAGE" || log.Environment == "PROD" : log.Environment == environment)
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
                case "logType":
                    orderedLogs = sortOrder == "asc" ? searchedLogs.OrderBy(pl => pl.Type) : searchedLogs.OrderByDescending(pl => pl.Type);
                    break;
                case "creationDate":
                    orderedLogs = sortOrder == "asc" ? searchedLogs.OrderBy(pl => pl.CreationDate) : searchedLogs.OrderByDescending(pl => pl.CreationDate);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Invalid sortBy = {sortBy} parameter");
            }

            var pagedLogs = orderedLogs.Skip(page * pageSize).Take(pageSize);

            return pagedLogs.Select(pl => new LogDtoGet(pl)).ToList();
        }

        //Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
        public void LogTraceMessage(string messageToLog)
        {
            Log(new LogDtoPost
            {
                Application = _logSettings.Application,
                Project = _logSettings.Project,
                CorrelationId = _logSettings.CorrelationId,
                Text = messageToLog,
                Type = LogType.Trace,
                Environment = _logSettings.Environment
            }, false);
        }

        //Information messages, which are normally enabled in production environment
        public void LogInfoMessage(string messageToLog)
        {
            Log(new LogDtoPost
            {
                Application = _logSettings.Application,
                Project = _logSettings.Project,
                CorrelationId = _logSettings.CorrelationId,
                Text = messageToLog,
                Type = LogType.Info,
                Environment = _logSettings.Environment
            }, false);
        }

        //Error messages - most of the time these are Exceptions
        public void LogErrorMessage(string messageToLog)
        {
            Log(new LogDtoPost
            {
                Application = _logSettings.Application,
                Project = _logSettings.Project,
                CorrelationId = _logSettings.CorrelationId,
                Text = messageToLog,
                Type = LogType.Error,
                Environment = _logSettings.Environment
            }, false);
        }
    }
}
