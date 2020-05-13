using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Extensions;
using Umbraco.Web.Common.Routing;

namespace Umbraco.Web.Common.Install
{

    public class InstallAreaRoutes : IAreaRoutes
    {
        private readonly IRuntimeState _runtime;
        private readonly ILogger _logger;
        private readonly UriUtility _uriUtility;
        private readonly LinkGenerator _linkGenerator;

        public InstallAreaRoutes(IRuntimeState runtime, ILogger logger, UriUtility uriUtility, LinkGenerator linkGenerator)
        {
            _runtime = runtime;
            _logger = logger;
            _uriUtility = uriUtility;
            _linkGenerator = linkGenerator;
        }

        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            var installPath = _uriUtility.ToAbsolute(Umbraco.Core.Constants.SystemDirectories.Install).EnsureEndsWith('/');

            switch (_runtime.Level)
            {
                case RuntimeLevel.Install:
                case RuntimeLevel.Upgrade:
                    endpoints.MapAreaControllerRoute(
                        "umbraco-install-api",
                        Umbraco.Core.Constants.Web.Mvc.InstallArea,
                        $"{installPath}api/{{Action}}",
                        new { controller = ControllerExtensions.GetControllerName<InstallApiController>() });

                    endpoints.MapAreaControllerRoute(
                        "umbraco-install",
                        Umbraco.Core.Constants.Web.Mvc.InstallArea,
                        $"{installPath}{{controller}}/{{Action}}",
                        new { controller = ControllerExtensions.GetControllerName<InstallController>(), action = "Index" });

                    // TODO: Potentially switch this to dynamic routing so we can essentially disable/overwrite the back office routes to redirect to install,
                    // example https://www.strathweb.com/2019/08/dynamic-controller-routing-in-asp-net-core-3-0/

                    // register catch all because if we are in install/upgrade mode then we'll catch everything and redirect
                    endpoints.MapGet("{*url}", context =>
                    {
                        var uri = context.Request.GetEncodedUrl();
                        // redirect to install
                        ReportRuntime(_logger, _runtime.Level, "Umbraco must install or upgrade.");

                        var installUrl = $"{installPath}?redir=true&url={uri}";
                        context.Response.Redirect(installUrl, false);
                        return Task.CompletedTask;
                    });
                    break;
                case RuntimeLevel.Run:

                    // when we are in run mode redirect to the back office if the installer endpoint is hit
                    endpoints.MapGet($"{installPath}{{controller?}}/{{Action?}}", context =>
                    {
                        // redirect to umbraco
                        context.Response.Redirect(_linkGenerator.GetBackOfficeUrl(), false);
                        return Task.CompletedTask;
                    });

                    break;
                case RuntimeLevel.BootFailed:
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                    break;
                
            }
        }

        private static bool _reported;
        private static RuntimeLevel _reportedLevel;

        private static void ReportRuntime(ILogger logger, RuntimeLevel level, string message)
        {
            if (_reported && _reportedLevel == level) return;
            _reported = true;
            _reportedLevel = level;
            logger.Warn(typeof(UmbracoInstallApplicationBuilderExtensions), message);
        }
    }
}
