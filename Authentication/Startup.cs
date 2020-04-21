using Microsoft.Extensions.Configuration;
using Shared.WebApi;

namespace Authentication
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }
    }
}
