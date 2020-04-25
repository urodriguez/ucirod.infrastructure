using Newtonsoft.Json.Linq;
using Shared.Infrastructure.CrossCutting.Authentication;

namespace Reporting
{
    public class ReportDto
    {
        public Credential Credential { get; set; }
        public string Template { get; set; }
        public string Data { get; set; }
    }
}