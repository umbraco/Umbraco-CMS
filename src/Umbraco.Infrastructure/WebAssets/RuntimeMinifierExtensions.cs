using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.WebAssets;

namespace Umbraco.Web.JavaScript
{
    public static class RuntimeMinifierExtensions
    {
        /// <summary>
        /// Returns the JavaScript to load the back office's assets
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetScriptForLoadingBackOfficeAsync(this IRuntimeMinifier minifier, IGlobalSettings globalSettings, IIOHelper ioHelper)
        {
            var initCss = new CssInitialization(minifier);
            var files = await minifier.GetAssetPathsAsync(BackOfficeWebAssets.UmbracoJsBundleName);
            var result = JavaScriptHelper.GetJavascriptInitialization(files, "umbraco", globalSettings, ioHelper);
            result += await initCss.GetStylesheetInitializationAsync();

            return result;
        }
    }
}
