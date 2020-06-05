using Hangfire.Dashboard;

namespace Logging
{
    public class LoggingHangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var cookies = httpContext.Request.Cookies;
            var infrastructureHfDbCookie = cookies["infrastructure_hf_dashboard_cookie"];

            if (string.IsNullOrEmpty(infrastructureHfDbCookie)) return false;

            return infrastructureHfDbCookie.Equals("1nfr45tructur3_h4ngf1r3_d45hb0rd");
        }
    }
}