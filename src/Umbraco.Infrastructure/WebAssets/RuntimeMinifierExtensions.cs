using System;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;

namespace Umbraco.Web.WebAssets
{
    public static class RuntimeMinifierExtensions
    {
        /// <summary>
        /// Returns the JavaScript to load the back office's assets
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetScriptForLoadingBackOfficeAsync(this IRuntimeMinifier minifier, GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            var files = await minifier.GetAssetPathsAsync(BackOfficeWebAssets.UmbracoJsBundleName);
            var result = BackOfficeJavaScriptInitializer.GetJavascriptInitialization(files, "umbraco", globalSettings, hostingEnvironment);
            result += await GetStylesheetInitializationAsync(minifier);

            return result;
        }

        /// <summary>
        /// Gets the back office css bundle paths and formats a JS call to lazy load them
        /// </summary>
        private static async Task<string> GetStylesheetInitializationAsync(IRuntimeMinifier minifier)
        {
            var files = await minifier.GetAssetPathsAsync(BackOfficeWebAssets.UmbracoCssBundleName);
            var sb = new StringBuilder();
            foreach (var file in files)
                sb.AppendFormat("{0}LazyLoad.css('{1}');", Environment.NewLine, file);
            return sb.ToString();
        }

    }
}
