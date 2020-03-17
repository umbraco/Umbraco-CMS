using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Assets;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Runtime;
using CssFile = Umbraco.Core.Assets.CssFile;

namespace Umbraco.Web.JavaScript
{
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
        /// <param name="httpContext"></param>
        /// <param name="scripts"></param>
        /// <param name="angularModule">
        /// The angular module name to boot
        /// </param>
        /// <returns></returns>
        public static string GetJavascriptInitialization(HttpContextBase httpContext, IEnumerable<string> scripts, string angularModule, IGlobalSettings globalSettings, IIOHelper ioHelper)
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

        internal static IEnumerable<string> GetTinyMceInitialization()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.TinyMceInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString());
        }

        internal static IEnumerable<string> OptimizeTinyMceScriptFiles(HttpContextBase httpContext, IRuntimeMinifier runtimeMinifier)
        {
            return OptimizeScriptFiles(httpContext, GetTinyMceInitialization(), runtimeMinifier);
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
        /// Returns a list of optimized script paths
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="scriptFiles"></param>
        /// <param name="runtimeMinifier"></param>
        /// <returns></returns>
        /// <remarks>
        /// Used to cache bust and optimize script paths
        /// </remarks>
        public static IEnumerable<string> OptimizeScriptFiles(HttpContextBase httpContext, IEnumerable<string> scriptFiles, IRuntimeMinifier runtimeMinifier)
        {
            var scripts = new HashSet<string>();
            foreach (var script in scriptFiles)
                scripts.Add(script);

            scripts = new HashSet<string>(OptimizeAssetCollection(scripts, AssetType.Javascript, httpContext, runtimeMinifier));

            return scripts.ToArray();
        }

        internal static IEnumerable<string> OptimizeAssetCollection(IEnumerable<string> assets, AssetType assetType, HttpContextBase httpContext, IRuntimeMinifier runtimeMinifier)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var requestUrl = httpContext.Request.Url;
            if (requestUrl == null) throw new ArgumentException("HttpContext.Request.Url is null.", nameof(httpContext));

            var dependencies = assets.Where(x => x.IsNullOrWhiteSpace() == false).Select(x =>
            {
                // most declarations with be made relative to the /umbraco folder, so things
                // like lib/blah/blah.js so we need to turn them into absolutes here
                if (x.StartsWith("/") == false && Uri.IsWellFormedUriString(x, UriKind.Relative))
                {
                    return new AssetFile(assetType) { FilePath = new Uri(requestUrl, x).AbsolutePath };
                }

                return assetType == AssetType.Javascript
                    ? new JavaScriptFile(x)
                    : new CssFile(x) as IAssetFile;
            }).ToList();


            return runtimeMinifier.GetAssetPaths(assetType, dependencies);;
        }
    }
}
