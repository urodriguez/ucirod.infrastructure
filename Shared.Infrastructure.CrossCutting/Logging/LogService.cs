﻿using System;
using System.IO;
using System.Threading.Tasks;
using RestSharp;
using Shared.Infrastructure.CrossCutting.AppSettings;

namespace Shared.Infrastructure.CrossCutting.Logging
{
    public class LogService : ILogService
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly IRestClient _restClient;

        private string _project;
        private Guid _correlationId;

        public LogService(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
            _restClient = new RestClient(_appSettingsService.LoggingUrl);
        }

        public Guid GetCorrelationId() => _correlationId;

        //Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
        public void LogTraceMessage(string messageToLog)
        {
            LogMessage(messageToLog, LogType.Trace);
        }

        //Information messages, which are normally enabled in production environment
        public void LogInfoMessage(string messageToLog)
        {
            LogMessage(messageToLog, LogType.Info);
        }

        //Error messages - most of the time these are Exceptions
        public void LogErrorMessage(string messageToLog)
        {
            LogMessage(messageToLog, LogType.Error);
        }

        private void LogMessage(string messageToLog, LogType logType)
        {
            if (string.IsNullOrEmpty(_project)) throw new Exception("LogService was not configured correctly. Use 'UseProject' method to configure 'Project' field");

            GenerateCorrelationId();

            var task = new Task(() =>
            {
                try
                {
                    var request = new RestRequest("logs", Method.POST);

                    var log = new LogDto
                    {
                        Credential = _appSettingsService.Credential,
                        Application = "Infrastructure",
                        Project = _project,
                        CorrelationId = _correlationId,
                        Text = messageToLog,
                        Type = logType,
                        Environment = _appSettingsService.Environment.Name
                    };
                    request.AddJsonBody(log);

                    var logResponse = _restClient.Post(request);

                    if (!logResponse.IsSuccessful)
                        throw new Exception(
                            $"logResponse.IsSuccessful=false - logResponse.StatusCode={logResponse.StatusCode} - logResponse.Content={logResponse.Content}"
                        );
                }
                catch (Exception e)
                {
                    //Do not call LogService to log this exception in order to avoid infinite loop
                    FileSystemLog($"{e}");

                    //queue 'log' data

                    //do not throw the exception in order to avoid finish the main request
                }
            });

            task.Start();
        }

        private void GenerateCorrelationId()
        {
            _correlationId = _correlationId == Guid.Empty ? Guid.NewGuid() : _correlationId;
        }

        private void FileSystemLog(string messageToLog)
        {
            var fileSystemLogsDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}FileSystemLogs";
            Directory.CreateDirectory(fileSystemLogsDirectory);

            var logFileName = $"FSL,{_correlationId}";
            var logFilePath = $"{fileSystemLogsDirectory}\\{logFileName}.txt";

            using (StreamWriter sw = File.CreateText(logFilePath))
            {
                sw.WriteLine(messageToLog);
            }
        }

        public void UseProject(string project)
        {
            _project = project;
        }
    }
}