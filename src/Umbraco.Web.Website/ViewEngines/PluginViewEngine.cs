using System.Diagnostics;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Umbraco.Web.Website.ViewEngines
{
    // TODO: We don't really need to have different view engines simply to search additional places,
    // we can just do ConfigureOptions<RazorViewEngineOptions> on startup to add more to the
    // default list so this can be totally removed/replaced with configure options logic.

    /// <summary>
    /// A view engine to look into the App_Plugins folder for views for packaged controllers
    /// </summary>
    public class PluginViewEngine : RazorViewEngine, IPluginViewEngine
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

        private static IOptions<RazorViewEngineOptions> OverrideViewLocations() => Options.Create(new RazorViewEngineOptions()
        {
            // This is definitely not doing what it used to do :P see:
            // https://github.com/umbraco/Umbraco-CMS/blob/v8/contrib/src/Umbraco.Web/Mvc/PluginViewEngine.cs#L23

            AreaViewLocationFormats =
                {
                    // set all of the area view locations to the plugin folder
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.cshtml"),
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.cshtml"),

                    // will be used when we have partial view and child action macros
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/Partials/{0}.cshtml"),
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/MacroPartials/{0}.cshtml"),
                    // for partialsCurrent.
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
