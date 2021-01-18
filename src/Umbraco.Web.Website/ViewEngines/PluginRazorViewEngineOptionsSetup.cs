using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace Umbraco.Web.Website.ViewEngines
{
    /// <summary>
    /// Configure view engine locations for front-end rendering based on App_Plugins views
    /// </summary>
    public class PluginRazorViewEngineOptionsSetup : IConfigureOptions<RazorViewEngineOptions>
    {
        /// <inheritdoc/>
        public void Configure(RazorViewEngineOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        }

        /// <summary>
        /// Expands the default view locations
        /// </summary>
        private class ViewLocationExpander : IViewLocationExpander
        {
            public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
            {
                string[] umbViewLocations = new string[]
                {
                    // area view locations for the plugin folder
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.cshtml"),
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.cshtml"),

                    // will be used when we have partial view and child action macros
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/Partials/{0}.cshtml"),
                    string.Concat(Core.Constants.SystemDirectories.AppPlugins, "/{2}/Views/MacroPartials/{0}.cshtml")
                };

                viewLocations = umbViewLocations.Concat(viewLocations);

                return viewLocations;
            }

            // not a dynamic expander
            public void PopulateValues(ViewLocationExpanderContext context) { }
        }
    }
}
