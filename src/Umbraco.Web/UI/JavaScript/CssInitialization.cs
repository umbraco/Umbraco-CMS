using System.Web;
using ClientDependency.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;

namespace Umbraco.Web.UI.JavaScript
{
    internal class CssInitialization : AssetInitialization
    {
        private readonly ManifestParser _parser;
        public CssInitialization(ManifestParser parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// Processes all found manifest files and outputs yepnope.injectcss calls for all css files found in all manifests
        /// </summary>
        public string GetStylesheetInitialization(HttpContextBase httpContext)
        {
            var result = GetStylesheetInitializationArray(httpContext);

            return ParseMain(result);
        }

        public JArray GetStylesheetInitializationArray(HttpContextBase httpContext)
        {
            var merged = new JArray();    
            foreach (var m in _parser.GetManifests())
            {
                ManifestParser.MergeJArrays(merged, m.StylesheetInitialize);
            }

            //now we can optimize if in release mode
            merged = OptimizeAssetCollection(merged, ClientDependencyType.Css, httpContext);

            //now we need to merge in any found cdf declarations on property editors
            ManifestParser.MergeJArrays(merged, ScanPropertyEditors(ClientDependencyType.Css, httpContext));

            return merged;
        }

       
        /// <summary>
        /// Parses the CssResources.Main and returns a yepnop.injectCss format
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        internal static string ParseMain(JArray files)
        {
            var sb = new StringBuilder();
            foreach (var file in files)
                sb.AppendFormat("{0}LazyLoad.css('{1}');", Environment.NewLine, file);

            return sb.ToString();
        }

    }
}
