using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.WebAssets;

namespace Umbraco.Web.JavaScript
{
    public class CssInitialization
    {
        private readonly IRuntimeMinifier _runtimeMinifier;

        public CssInitialization(IRuntimeMinifier runtimeMinifier)
        {
            _runtimeMinifier = runtimeMinifier;
        }

        /// <summary>
        /// Processes all found manifest files, and outputs css inject calls for all css files found in all manifests.
        /// </summary>
        public async Task<string> GetStylesheetInitializationAsync()
        {
            var files = await _runtimeMinifier.GetAssetPathsAsync(BackOfficeWebAssets.UmbracoCssBundleName);
            return WriteScript(files);
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
