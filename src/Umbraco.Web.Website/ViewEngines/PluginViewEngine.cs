using System.Diagnostics;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Umbraco.Web.Website.ViewEngines
{
    /// <summary>
    /// A view engine to look into the App_Plugins folder for views for packaged controllers
    /// </summary>
    public class PluginViewEngine : RazorViewEngine
    {
        public PluginViewEngine(
            IRazorPageFactoryProvider pageFactory,
            IRazorPageActivator pageActivator,
            HtmlEncoder htmlEncoder,
            ILoggerFactory loggerFactory,
            DiagnosticListener diagnosticListener)
            : base(pageFactory, pageActivator, htmlEncoder, OverrideViewLocations(), loggerFactory, diagnosticListener)
        {
        }

        private static IOptions<RazorViewEngineOptions> OverrideViewLocations()
        {
            return Options.Create(new RazorViewEngineOptions()
            {
                AreaViewLocationFormats =
                {
                    //set all of the area view locations to the plugin folder
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.cshtml"),
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.cshtml"),

                    //will be used when we have partial view and child action macros
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/Partials/{0}.cshtml"),
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/MacroPartials/{0}.cshtml"),
                    //for partialsCurrent.
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.cshtml"),
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.cshtml"),
                },
                ViewLocationFormats =
                {
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.cshtml"),
                }
            });
        }
    }
}
