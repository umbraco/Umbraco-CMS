using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Assets;
using Umbraco.Core.Manifest;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.JavaScript
{
    /// <summary>
    /// Reads from all defined manifests and ensures that any of their initialization is output with the
    /// main Umbraco initialization output.
    /// </summary>
    internal class JsInitialization : AssetInitialization
    {
        private readonly IManifestParser _parser;
        private readonly IRuntimeMinifier _runtimeMinifier;

        public JsInitialization(IManifestParser parser, IRuntimeMinifier runtimeMinifier) : base(runtimeMinifier)
        {
            _parser = parser;
            _runtimeMinifier = runtimeMinifier;
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

            scripts = new HashSet<string>(JavaScriptHelper.OptimizeAssetCollection(scripts, AssetType.Javascript, httpContext, _runtimeMinifier));

            foreach (var script in ScanPropertyEditors(AssetType.Javascript, httpContext))
                scripts.Add(script);

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
    }
}
