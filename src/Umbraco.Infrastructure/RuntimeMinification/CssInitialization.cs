using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Assets;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.JavaScript
{
    public class CssInitialization : AssetInitialization
    {
        private readonly IManifestParser _parser;
        private readonly IRuntimeMinifier _runtimeMinifier;

        public CssInitialization(
            IManifestParser parser,
            IRuntimeMinifier runtimeMinifier,
            PropertyEditorCollection propertyEditorCollection)
            : base(runtimeMinifier, propertyEditorCollection)
        {
            _parser = parser;
            _runtimeMinifier = runtimeMinifier;
        }

        /// <summary>
        /// Processes all found manifest files, and outputs css inject calls for all css files found in all manifests.
        /// </summary>
        public async Task<string> GetStylesheetInitializationAsync(Uri requestUrl)
        {
            var files = await GetStylesheetFilesAsync(requestUrl);
            return WriteScript(files);
        }

        public async Task<IEnumerable<string>> GetStylesheetFilesAsync(Uri requestUrl)
        {
            var stylesheets = new HashSet<string>();
            var optimizedManifest = await JavaScriptHelper.OptimizeAssetCollectionAsync(_parser.Manifest.Stylesheets, AssetType.Css, requestUrl, _runtimeMinifier);
            foreach (var stylesheet in optimizedManifest)
                stylesheets.Add(stylesheet);

            foreach (var stylesheet in await ScanPropertyEditorsAsync(AssetType.Css))
                stylesheets.Add(stylesheet);

            return stylesheets.ToArray();
        }

        internal static string WriteScript(IEnumerable<string> files)
        {
            var sb = new StringBuilder();
            foreach (var file in files)
                sb.AppendFormat("{0}LazyLoad.css('{1}');", Environment.NewLine, file);
            return sb.ToString();
        }

    }
}
