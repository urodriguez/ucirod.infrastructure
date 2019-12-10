using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace Infrastructure.CrossCutting.LogService
{
    public class LogService : ILogService
    {
        private readonly IRestClient _restClient;

        private readonly string _application;
        private readonly string _project;
        private readonly Guid _correlationId;

        public LogService(string application, string project, string env)
        {
            _application = application;
            _project = project;

            var envUrl = new Dictionary<string, string>
            {
                { "DEV", "http://www.ucirod.logging-test.com:40000/api" },
                { "TEST", "http://www.ucirod.logging-test.com:40000/api" },
                { "STAGE", "http://www.ucirod.logging.com/api" },
                { "PROD", "http://www.ucirod.logging.com/api" }
            };

            _restClient = new RestClient(envUrl[env]);

            var request = new RestRequest("correlations", Method.POST);
            var correlationResponse = _restClient.Post<Correlation>(request);

            _correlationId = correlationResponse.Data.Id;
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
            var task = new Task(() =>
            {
                try
                {
                    var request = new RestRequest("logs", Method.POST);

                    var log = new Log
                    {
                        Application = _application,
                        Project = _project,
                        CorrelationId = _correlationId,
                        Text = messageToLog,
                        Type = logType
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
