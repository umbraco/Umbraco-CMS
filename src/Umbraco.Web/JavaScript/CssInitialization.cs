using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core.Assets;
using Umbraco.Core.Manifest;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.JavaScript
{
    internal class CssInitialization : AssetInitialization
    {
        private readonly IManifestParser _parser;
        private readonly IRuntimeMinifier _runtimeMinifier;

        public CssInitialization(IManifestParser parser, IRuntimeMinifier runtimeMinifier) : base(runtimeMinifier)
        {
            _parser = parser;
            _runtimeMinifier = runtimeMinifier;
        }

        /// <summary>
        /// Processes all found manifest files, and outputs css inject calls for all css files found in all manifests.
        /// </summary>
        public string GetStylesheetInitialization(HttpContextBase httpContext)
        {
            var files = GetStylesheetFiles(httpContext);
            return WriteScript(files);
        }

        public IEnumerable<string> GetStylesheetFiles(HttpContextBase httpContext)
        {
            var stylesheets = new HashSet<string>();
            var optimizedManifest = OptimizeAssetCollection(_parser.Manifest.Stylesheets, AssetType.Css, httpContext, _runtimeMinifier);
            foreach (var stylesheet in optimizedManifest)
                stylesheets.Add(stylesheet);

            foreach (var stylesheet in ScanPropertyEditors(AssetType.Css, httpContext))
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
