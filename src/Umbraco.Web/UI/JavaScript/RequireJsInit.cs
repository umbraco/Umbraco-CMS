using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Manifest;

namespace Umbraco.Web.UI.JavaScript
{
    /// <summary>
    /// Reads from all defined manifests and ensures that any of their initialization is output with the
    /// main Umbraco initialization output.
    /// </summary>
    internal class RequireJsInit
    {
        private readonly DirectoryInfo _pluginsDir;
        private readonly ManifestParser _parser;

        public RequireJsInit(DirectoryInfo pluginsDir)
        {
            _pluginsDir = pluginsDir;
            _parser = new ManifestParser(_pluginsDir);
        }

        private static readonly Regex Token = new Regex("(\"##\\w+?##\")", RegexOptions.Compiled);

        /// <summary>
        /// Processes all found manifest files and outputs the main.js file containing all plugin manifests
        /// </summary>
        public string GetJavascriptInitialization(JObject umbracoConfig, JArray umbracoInit)
        {
            foreach (var m in _parser.GetManifests())
            {
                ManifestParser.MergeJObjects(umbracoConfig, m.JavaScriptConfig, true);
                ManifestParser.MergeJArrays(umbracoInit, m.JavaScriptInitialize);
            }

            return ParseMain(umbracoConfig.ToString(), umbracoInit.ToString());
        }

        /// <summary>
        /// Returns the default config as a JObject
        /// </summary>
        /// <returns></returns>
        internal static JObject GetDefaultConfig()
        {
            var config = Resources.RequireJsConfig;
            var jObj = JsonConvert.DeserializeObject<JObject>(config);
            return jObj;
        }

        /// <summary>
        /// Returns the default config as a JArray
        /// </summary>
        /// <returns></returns>
        internal static JArray GetDefaultInitialization()
        {
            var init = Resources.RequireJsInitialize;
            var jArr = JsonConvert.DeserializeObject<JArray>(init);
            return jArr;
        }

        /// <summary>
        /// Parses the JsResources.Main and replaces the replacement tokens accordingly.
        /// </summary>
        /// <param name="replacements"></param>
        /// <returns></returns>
        internal static string ParseMain(params string[] replacements)
        {
            var count = 0;
            return Token.Replace(Resources.Main, match =>
            {
                var replaced = replacements[count];
                count++;
                return replaced;
            });
        }

    }
}
