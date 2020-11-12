using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Umbraco.Web.Website.ViewEngines
{
    public class PluginMvcViewOptionsSetup : IConfigureOptions<MvcViewOptions>
    {
        private readonly IPluginViewEngine _pluginViewEngine;

        public PluginMvcViewOptionsSetup(IPluginViewEngine pluginViewEngine)
        {
            _pluginViewEngine = pluginViewEngine ?? throw new ArgumentNullException(nameof(pluginViewEngine));
        }

        public void Configure(MvcViewOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.ViewEngines.Add(_pluginViewEngine);
        }
    }
}
