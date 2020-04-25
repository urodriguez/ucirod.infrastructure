using System;
using Newtonsoft.Json.Linq;

namespace Reporting.Domain
{
    public class Report
    {
        public string Template { get; set; }
        public JObject Data { get; set; }

        public Report(string template, string data)
        {
            if (string.IsNullOrEmpty(template)) throw new ArgumentNullException("'template' field can not be null or empty");

            Template = template;

            try
            {
                Data = JObject.Parse(data);
            }
            catch (Exception)
            {
                throw new FormatException($"An error has occurred trying to parse Report.Data. Check Json format");
            }
        }
    }
}