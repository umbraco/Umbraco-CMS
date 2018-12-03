using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using ClientDependency.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using System.Linq;
using System.Text;

namespace Umbraco.Web.UI.JavaScript
{
    /// <summary>
    /// Reads from all defined manifests and ensures that any of their initialization is output with the
    /// main Umbraco initialization output.
    /// </summary>
    internal class JsInitialization : AssetInitialization
    {
        private readonly ManifestParser _parser;

        public JsInitialization(ManifestParser parser)
        {
            _parser = parser;
        }

        // deal with javascript functions inside of json (not a supported json syntax)
        private const string PrefixJavaScriptObject = "@@@@";
        private static readonly Regex JsFunctionParser = new Regex($"(\"{PrefixJavaScriptObject}(.*?)\")+",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // replace tokens in the js main
        private static readonly Regex Token = new Regex("(\"##\\w+?##\")", RegexOptions.Compiled);

        /// <summary>
        /// Gets the JS initialization script to boot the back office application
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="scripts"></param>
        /// <param name="angularModule">
        /// The angular module name to boot
        /// </param>
        /// <returns></returns>
        public static string GetJavascriptInitialization(HttpContextBase httpContext, IEnumerable<string> scripts, string angularModule)
        {
            var jarray = new StringBuilder();
            jarray.AppendLine("[");
            var first = true;
            foreach (var file in scripts)
            {
                if (first) first = false;
                else jarray.AppendLine(",");
                jarray.Append("\"");
                jarray.Append(file);
                jarray.Append("\"");

            }
            jarray.Append("]");

            return WriteScript(jarray.ToString(), IOHelper.ResolveUrl(SystemDirectories.Umbraco), angularModule);
        }

        /// <summary>
        /// Returns a list of optimized script paths for the back office
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="umbracoInit"></param>
        /// <param name="additionalJsFiles"></param>
        /// <returns>
        /// Cache busted/optimized script paths for the back office including manifest and property editor scripts
        /// </returns>
        /// <remarks>
        /// Used to cache bust and optimize script paths for the back office
        /// </remarks>
        public IEnumerable<string> OptimizeBackOfficeScriptFiles(HttpContextBase httpContext, IEnumerable<string> umbracoInit, IEnumerable<string> additionalJsFiles = null)
        {
            var scripts = new HashSet<string>();
            foreach (var script in umbracoInit)
                scripts.Add(script);
            foreach (var script in _parser.Manifest.Scripts)
                scripts.Add(script);
            if (additionalJsFiles != null)
                foreach (var script in additionalJsFiles)
                    scripts.Add(script);

            scripts = new HashSet<string>(OptimizeAssetCollection(scripts, ClientDependencyType.Javascript, httpContext));

            foreach (var script in ScanPropertyEditors(ClientDependencyType.Javascript, httpContext))
                scripts.Add(script);

            return scripts.ToArray();
        }

        /// <summary>
        /// Returns a list of optimized script paths
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="scriptFiles"></param>
        /// <returns></returns>
        /// <remarks>
        /// Used to cache bust and optimize script paths
        /// </remarks>
        public static IEnumerable<string> OptimizeScriptFiles(HttpContextBase httpContext, IEnumerable<string> scriptFiles)
        {
            var scripts = new HashSet<string>();
            foreach (var script in scriptFiles)
                scripts.Add(script);

            scripts = new HashSet<string>(OptimizeAssetCollection(scripts, ClientDependencyType.Javascript, httpContext));

            return scripts.ToArray();
        }

        /// <summary>
        /// Returns the default config as a JArray
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<string> GetDefaultInitialization()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.JsInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString());
        }

        /// <summary>
        /// Returns the default config as a JArray
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<string> GetPreviewInitialization()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.PreviewInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString());
        }

        /// <summary>
        /// Parses the JsResources.Main and replaces the replacement tokens accordingly.
        /// </summary>
        /// <param name="replacements"></param>
        /// <returns></returns>
        internal static string WriteScript(string scripts, string umbracoPath, string angularModule)
        {
            var count = 0;
            var replacements = new[] { scripts, umbracoPath, angularModule };
            // replace, catering for the special syntax when we have
            // js function() objects contained in the json

            return Token.Replace(Resources.Main, match =>
            {
                var replacement = replacements[count++];
                return JsFunctionParser.Replace(replacement, "$2");
            });
        }
    }
}
