using System;
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
        private readonly Guid _correlationId;

        public LogService(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;

            _restClient = new RestClient(_appSettingsService.LoggingUrl);

            var request = new RestRequest("correlations", Method.POST);
            var correlationResponse = _restClient.Post<Correlation>(request);

            _correlationId = correlationResponse.Data.Id;
        }

        public void UseProject(string project)
        {
            _project = project;
        }

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

                    _restClient.Post(request);
                }
                catch (Exception e)
                {
                    //TODO: queue to resend 
                }
            });

            task.Start();
        }
    }
}