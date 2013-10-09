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
    internal class CssInitialization
    {
        private readonly ManifestParser _parser;

        public CssInitialization(ManifestParser parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// Processes all found manifest files and outputs yepnope.injectcss calls for all css files found in all manifests
        /// </summary>
        public string GetStylesheetInitialization()
        {
            JArray merged = new JArray();    
            foreach (var m in _parser.GetManifests())
            {
                ManifestParser.MergeJArrays(merged, m.StylesheetInitialize);
            }

            return ParseMain(merged);
        }

       
        /// <summary>
        /// Parses the CssResources.Main and returns a yepnop.injectCss format
        /// </summary>
        /// <param name="replacements"></param>
        /// <returns></returns>
        internal static string ParseMain(JArray files)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var file in files)
                sb.AppendFormat("{0}yepnope.injectCss('{1}');", Environment.NewLine, file);

            return sb.ToString();
        }

    }
}
