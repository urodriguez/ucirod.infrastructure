using Microsoft.Extensions.Configuration;
using Shared.WebApi;

namespace Reporting
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }
    }
}
