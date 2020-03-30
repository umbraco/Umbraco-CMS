using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Assets;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Runtime;
using Umbraco.Infrastructure.RuntimeMinification;

namespace Umbraco.Web.JavaScript
{
    /// <summary>
    /// Reads from all defined manifests and ensures that any of their initialization is output with the
    /// main Umbraco initialization output.
    /// </summary>
    public class JsInitialization : AssetInitialization
    {
        private readonly IManifestParser _parser;
        private readonly IRuntimeMinifier _runtimeMinifier;

        public JsInitialization(IManifestParser parser, IRuntimeMinifier runtimeMinifier, PropertyEditorCollection propertyEditorCollection) : base(runtimeMinifier, propertyEditorCollection)
        {
            _parser = parser;
            _runtimeMinifier = runtimeMinifier;
        }

        /// <summary>
        /// Returns a list of optimized script paths for the back office
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="umbracoInit"></param>
        /// <param name="additionalJsFiles"></param>
        /// <returns>
        /// Cache busted/optimized script paths for the back office including manifest and property editor scripts
        /// </returns>
        /// <remarks>
        /// Used to cache bust and optimize script paths for the back office
        /// </remarks>
        public async Task<IEnumerable<string>> OptimizeBackOfficeScriptFilesAsync(Uri requestUrl, IEnumerable<string> umbracoInit, IEnumerable<string> additionalJsFiles = null)
        {
            var scripts = new HashSet<string>();
            foreach (var script in umbracoInit)
                scripts.Add(script);
            foreach (var script in _parser.Manifest.Scripts)
                scripts.Add(script);
            if (additionalJsFiles != null)
                foreach (var script in additionalJsFiles)
                    scripts.Add(script);

            scripts = new HashSet<string>(await JavaScriptHelper.OptimizeAssetCollectionAsync(scripts, AssetType.Javascript, requestUrl, _runtimeMinifier));

            foreach (var script in await ScanPropertyEditorsAsync(AssetType.Javascript))
                scripts.Add(script);

            return scripts.ToArray();
        }

        /// <summary>
        /// Returns the default config as a JArray
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetDefaultInitialization()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.JsInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString());
        }
    }
}
