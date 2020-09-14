using Rendering.Domain;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Rendering
{
    public class TemplateDto
    {
        public Credential Credential { get; set; }
        public string Content { get; set; }
        public string DataBound { get; set; }
        public TemplateType Type { get; set; }
        public RenderAs RenderAs { get; set; }
    }
}