using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using System.Linq;

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

        //used to strip comments
        internal static readonly Regex Comments = new Regex("(/\\*.*\\*/)", RegexOptions.Compiled);
        //used for dealing with js functions inside of json (which is not a supported json syntax)
        private const string PrefixJavaScriptObject = "@@@@";
        private static readonly Regex JsFunctionParser = new Regex(string.Format("(\"{0}(.*?)\")+", PrefixJavaScriptObject),
                                                                    RegexOptions.Multiline
                                                                    | RegexOptions.CultureInvariant
                                                                    | RegexOptions.Compiled);
        //used to replace the tokens in the js main
        private static readonly Regex Token = new Regex("(\"##\\w+?##\")", RegexOptions.Compiled);

        /// <summary>
        /// Processes all found manifest files and outputs the main.js file containing all plugin manifests
        /// </summary>
        public string GetJavascriptInitialization(HttpContextBase httpContext, JArray umbracoInit, JArray additionalJsFiles = null)
        {
            var result = GetJavascriptInitializationArray(httpContext, umbracoInit, additionalJsFiles);
                
            return ParseMain(
                result.ToString(),
                IOHelper.ResolveUrl(SystemDirectories.Umbraco));
        }

        public JArray GetJavascriptInitializationArray(HttpContextBase httpContext, JArray umbracoInit, JArray additionalJsFiles = null)
        {
            foreach (var m in _parser.GetManifests())
            {
                ManifestParser.MergeJArrays(umbracoInit, m.JavaScriptInitialize);
            }

            //merge in the additional ones specified if there are any
            if (additionalJsFiles != null)
            {
                ManifestParser.MergeJArrays(umbracoInit, additionalJsFiles);
            }

            //now we can optimize if in release mode
            umbracoInit = OptimizeAssetCollection(umbracoInit, ClientDependencyType.Javascript, httpContext);

            //now we need to merge in any found cdf declarations on property editors
            ManifestParser.MergeJArrays(umbracoInit, ScanPropertyEditors(ClientDependencyType.Javascript, httpContext));

            return umbracoInit;
        }

        /// <summary>
        /// Returns the default config as a JArray
        /// </summary>
        /// <returns></returns>
        internal static JArray GetDefaultInitialization()
        {
            var init = Resources.JsInitialize;
            var deserialized = JsonConvert.DeserializeObject<JArray>(init);
            var result = new JArray();
            foreach (var j in deserialized.Where(j => j.Type == JTokenType.String))
            {
                result.Add(j);
            }
            return result;
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

                //we need to cater for the special syntax when we have js function() objects contained in the json
                var jsFunctionParsed = JsFunctionParser.Replace(replaced, "$2");

                count++;

                return jsFunctionParsed;
            });
        }

    }
}
