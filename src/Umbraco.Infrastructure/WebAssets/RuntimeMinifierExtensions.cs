﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.WebAssets;

namespace Umbraco.Extensions
{
    public static class RuntimeMinifierExtensions
    {
        /// <summary>
        /// Returns the JavaScript to load the back office's assets
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetScriptForLoadingBackOfficeAsync(this IRuntimeMinifier minifier, GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            var coreScripts = await minifier.GetJsAssetPathsAsync(BackOfficeWebAssets.UmbracoCoreJsBundleName);
            var extensionsScripts = await minifier.GetJsAssetPathsAsync(BackOfficeWebAssets.UmbracoExtensionsJsBundleName);
            var result = BackOfficeJavaScriptInitializer.GetJavascriptInitialization(coreScripts.Union(extensionsScripts), "umbraco", globalSettings, hostingEnvironment);
            result += await GetStylesheetInitializationAsync(minifier);

            return result;
        }

        /// <summary>
        /// Gets the back office css bundle paths and formats a JS call to lazy load them
        /// </summary>
        private static async Task<string> GetStylesheetInitializationAsync(IRuntimeMinifier minifier)
        {
            var files = await minifier.GetCssAssetPathsAsync(BackOfficeWebAssets.UmbracoCssBundleName);
            var sb = new StringBuilder();
            foreach (var file in files)
                sb.AppendFormat("{0}LazyLoad.css('{1}');", Environment.NewLine, file);
            return sb.ToString();
        }

    }
}
