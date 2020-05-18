using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Logging.Application.Dtos;
using Logging.Domain;
using Logging.Infrastructure.Persistence;
using Shared.Application.Exceptions;
using Shared.Infrastructure.CrossCutting.AppSettings;
using Shared.Infrastructure.CrossCutting.Authentication;
using LogType = Logging.Domain.LogType;

namespace Logging.Application
{
    public class LogService : ILogService
    {
        private readonly LoggingDbContext _loggingDbContext;
        private readonly ICredentialService _credentialService;
        private readonly IAppSettingsService _appSettingsService;

        private string _internalCorrelationId;

        public LogService(LoggingDbContext loggingDbContext, ICredentialService credentialService, IAppSettingsService appSettingsService)
        {
            _loggingDbContext = loggingDbContext;
            _credentialService = credentialService;
            _appSettingsService = appSettingsService;

            //Internal correlation Id to track LogService methods execution
            _internalCorrelationId = Guid.NewGuid().ToString();
        }

        public string GetInternalCorrelationId() => _internalCorrelationId;

        public void Log(LogDtoPost logDto)
        {
            if (!_credentialService.AreValid(logDto.Credential))
            {
                if (logDto.Credential == null) throw new ArgumentNullException("Credential not provided");
                throw new AuthenticationFailException();
            }

            //this CorrelationId comes from outsite, it is not the same to "_internalCorrelationId"
            var log = new Log(logDto.Application, logDto.Project, logDto.CorrelationId, logDto.Text, logDto.Type, logDto.Environment);

            InsertLogIntoDatabase(log);
        }

        public IEnumerable<LogSearchResponseDto> Search(LogSearchRequestDto logSearchRequestDto)
        {
            if (!_credentialService.AreValid(logSearchRequestDto.Credential))
            {
                if (logSearchRequestDto.Credential == null) throw new ArgumentNullException("Credential not provided");
                throw new AuthenticationFailException();
            }

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

            return pagedLogs.Select(pl => new LogSearchResponseDto(pl)).ToList();
        }

        public void DeleteOldLogs()
        {
            //Remove logs from db
            try
            {
                InternalLogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Deleting Old Logs From DB | status=PENDING");

                _loggingDbContext.Logs.RemoveRange(_loggingDbContext.Logs.Where(l => l.CreationDate < DateTime.Today.AddDays(-7)));
                _loggingDbContext.SaveChanges();

                InternalLogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Deleting Old Logs From DB | status=FINISHED");
            }
            catch (Exception e)
            {
                InternalFileSystemLog($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Logging DB Exception | e={e}");
            }

            //Remove logs from file system
            try
            {
                InternalLogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Deleting Old Logs From FS");
                
                foreach (var fileSystemLogsDirectory in Directory.GetDirectories(_appSettingsService.FileSystemLogsDirectory))
                {
                    var directoryInfo = new DirectoryInfo(fileSystemLogsDirectory); 
                    InternalLogInfoMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Processing directory | directory.Name={directoryInfo.Name}");

                    var filesToDelete = directoryInfo.GetFiles("*.txt").Where(f => f.CreationTime < DateTime.Today.AddDays(-7));
                    var filesDeleted = 0;
                    foreach (var fileToDelete in filesToDelete)
                    {
                        fileToDelete.Delete();
                        filesDeleted++;
                    }

                    InternalLogInfoMessage(
                        filesToDelete.Count() != filesDeleted ?
                            $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Directory processed | status=INCOMPLED - directory.Name={directoryInfo.Name} - fileSystemLogsDirectory={fileSystemLogsDirectory} - filesToDelete={filesToDelete.Count()} - filesDeleted={filesDeleted}"
                            :
                            $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Directory processed | status=FINISHED - directory.Name={directoryInfo.Name} - fileSystemLogsDirectory={fileSystemLogsDirectory} - filesToDelete={filesToDelete.Count()} - filesDeleted={filesDeleted}"
                    );
                }
            }
            catch (Exception e)
            {
                InternalLogErrorMessage($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} | Logging FS Exception | e={e}");
            }
        }

        //Internal log for Logging in order to avoid infinite loop if Shared.Infrastructure.CrossCutting.Logging.LogService is called
        //Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
        public void InternalLogTraceMessage(string messageToLog)
        {
            var log = new Log("Infrasctructure", "Logging", _internalCorrelationId, messageToLog, LogType.Trace, _appSettingsService.Environment.Name);
            InternalLog(log);
        }

        //Internal log for Logging in order to avoid infinite loop if Shared.Infrastructure.CrossCutting.Logging.LogService is called
        //Information messages, which are normally enabled in production environment
        public void InternalLogInfoMessage(string messageToLog)
        {
            var log = new Log("Infrasctructure", "Logging", _internalCorrelationId, messageToLog, LogType.Info, _appSettingsService.Environment.Name);
            InternalLog(log);
        }

        //Internal log for Logging in order to avoid infinite loop if Shared.Infrastructure.CrossCutting.Logging.LogService is called
        //Error messages - most of the time these are Exceptions
        public void InternalLogErrorMessage(string messageToLog)
        {
            var log = new Log("Infrasctructure", "Logging", _internalCorrelationId, messageToLog, LogType.Error, _appSettingsService.Environment.Name);
            InternalLog(log);
        }

        private void InternalLog(Log log)
        {
            InsertLogIntoDatabase(log);
        }

        private void InsertLogIntoDatabase(Log log)
        {
            if (!log.HasTextToLog()) return;

            try
            {
                _loggingDbContext.Logs.Add(log);
                _loggingDbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new LoggingDbException(e);
            }
        }

        public void InternalFileSystemLog(string messageToLog)
        {
            var projFileSystemLogsDirectory = $"{_appSettingsService.FileSystemLogsDirectory}\\Logging";
            Directory.CreateDirectory(projFileSystemLogsDirectory);

            //add FSL code for File System Log
            _internalCorrelationId = $"FSL,{_internalCorrelationId}";

            var logFileName = _internalCorrelationId;
            var logFilePath = $"{projFileSystemLogsDirectory}\\{logFileName}.txt";

            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine($"{messageToLog}{Environment.NewLine}----------------******----------------{Environment.NewLine}");
            }
        }
    }
}
