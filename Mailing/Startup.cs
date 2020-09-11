using Microsoft.Extensions.Configuration;
using Shared.WebApiV3;

namespace Mailing
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }
    }
}