using System;
using Newtonsoft.Json.Linq;

namespace Rendering.Domain
{
    public class Template
    {
        public string Content { get; set; }
        public JObject DataBound { get; set; }
        public TemplateType Type { get; set; }
        public RenderAs RenderAs { get; set; }

        public Template(string content, string dataBound, TemplateType type, RenderAs renderAs)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentNullException($"{typeof(Template).Name}.Content field can not be null or empty");
            Content = content;

            try
            {
                DataBound = JObject.Parse(dataBound);
            }
            catch (Exception)
            {
                throw new FormatException($"An error has occurred trying to parse {typeof(Template).Name}.DataBound property. Check Json format");
            }

            if (!Enum.IsDefined(typeof(TemplateType), type)) throw new ArgumentOutOfRangeException($"{typeof(Template).Name}.Type code is invalid");
            Type = type;

            if (!Enum.IsDefined(typeof(RenderAs), renderAs)) throw new ArgumentOutOfRangeException($"{typeof(Template).Name}.RenderAs code is invalid");
            if (type == TemplateType.Pdf && renderAs != RenderAs.Bytes) throw new NotSupportedException($"{typeof(Template).Name}: 'Pdf' only can be rendered as bytes result");
            RenderAs = renderAs;
        }

        public string GetFileExtension()
        {
            return Type == TemplateType.PlainText ? "txt" : Type.ToString().ToLower();
        }
    }
}