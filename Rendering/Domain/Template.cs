using System;
using Newtonsoft.Json.Linq;

namespace Rendering.Domain
{
    public class Template
    {
        public string Content { get; set; }
        public JObject DataBound { get; set; }
        public OutputFormat OutputFormat { get; set; }

        public Template(string content, string dataBound, OutputFormat outputFormat)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentNullException($"{typeof(Template).Name}: 'content' field can not be null or empty");

            Content = content;

            try
            {
                DataBound = JObject.Parse(dataBound);
            }
            catch (Exception)
            {
                throw new FormatException("An error has occurred trying to parse Template.DataBound property. Check Json format");
            }

            OutputFormat = outputFormat;
        }

        public string GetOutputExtension()
        {
            return OutputFormat.ToString().ToLower();
        }
    }
}