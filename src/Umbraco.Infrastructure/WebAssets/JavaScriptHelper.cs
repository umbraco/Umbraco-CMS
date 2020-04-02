using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.WebAssets;
using Umbraco.Infrastructure.WebAssets;

namespace Umbraco.Web.JavaScript
{
    // TODO: Rename this
    public class JavaScriptHelper
    {
        // deal with javascript functions inside of json (not a supported json syntax)
        private const string PrefixJavaScriptObject = "@@@@";
        private static readonly Regex JsFunctionParser = new Regex($"(\"{PrefixJavaScriptObject}(.*?)\")+",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // replace tokens in the js main
        private static readonly Regex Token = new Regex("(\"##\\w+?##\")", RegexOptions.Compiled);


        /// <summary>
        /// Gets the JS initialization script to boot the back office application
        /// </summary>
        /// <param name="scripts"></param>
        /// <param name="angularModule">
        /// The angular module name to boot
        /// </param>
        /// <param name="globalSettings"></param>
        /// <param name="ioHelper"></param>
        /// <returns></returns>
        public static string GetJavascriptInitialization(IEnumerable<string> scripts, string angularModule, IGlobalSettings globalSettings, IIOHelper ioHelper)
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

            return WriteScript(jarray.ToString(), ioHelper.ResolveUrl(globalSettings.UmbracoPath), angularModule);
        }

        /// <summary>
        /// Parses the JsResources.Main and replaces the replacement tokens accordingly
        /// </summary>
        /// <param name="scripts"></param>
        /// <param name="umbracoPath"></param>
        /// <param name="angularModule"></param>
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
