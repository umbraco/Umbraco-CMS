using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace Umbraco.Extensions
{
    public static class UmbracoInstallApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables the Umbraco installer
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUmbracoInstaller(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                var runtime = app.ApplicationServices.GetRequiredService<IRuntimeState>();
                var logger = app.ApplicationServices.GetRequiredService<ILogger>();
                var uriUtility = app.ApplicationServices.GetRequiredService<UriUtility>();
                switch (runtime.Level)
                {
                    case RuntimeLevel.Install:
                    case RuntimeLevel.Upgrade:

                        var installPath = uriUtility.ToAbsolute(Constants.SystemDirectories.Install).EnsureEndsWith('/');

                        endpoints.MapAreaControllerRoute(
                            "umbraco-install-api",
                            Umbraco.Core.Constants.Web.Mvc.InstallArea,
                            $"{installPath}api/{{Action}}",
                            new { controller = "InstallApi" });

                        endpoints.MapAreaControllerRoute(
                            "umbraco-install",
                            Umbraco.Core.Constants.Web.Mvc.InstallArea,
                            $"{installPath}{{controller}}/{{Action}}",
                            new { controller = "Install", action = "Index" });

                        // TODO: Potentially switch this to dynamic routing so we can essentially disable/overwrite the back office routes to redirect to install,
                        // example https://www.strathweb.com/2019/08/dynamic-controller-routing-in-asp-net-core-3-0/

                        // register catch all because if we are in install/upgrade mode then we'll catch everything and redirect
                        endpoints.MapGet("{*url}", context =>
                        {
                            var uri = context.Request.GetEncodedUrl();
                            // redirect to install
                            ReportRuntime(logger, runtime.Level, "Umbraco must install or upgrade.");

                            var installUrl = $"{installPath}?redir=true&url={uri}";
                            context.Response.Redirect(installUrl, false);
                            return Task.CompletedTask;
                        });

                        break;
                }
            });

            return app;
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
