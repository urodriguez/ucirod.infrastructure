using Microsoft.Extensions.Configuration;
using Shared.WebApi;

namespace Mailing
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }
    }
}