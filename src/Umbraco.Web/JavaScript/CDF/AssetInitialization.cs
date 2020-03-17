using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Assets;
using Umbraco.Core.Runtime;
using Umbraco.Web.Composing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.JavaScript.CDF
{
    internal abstract class AssetInitialization
    {
        private readonly IRuntimeMinifier _runtimeMinifier;

        public AssetInitialization(IRuntimeMinifier runtimeMinifier)
        {
            _runtimeMinifier = runtimeMinifier;
        }

        protected IEnumerable<string> ScanPropertyEditors(AssetType assetType, HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            var attributes = Current.PropertyEditors
                    .SelectMany(x => x.GetType().GetCustomAttributes<PropertyEditorAssetAttribute>(false))
                    .Where(x => x.AssetType == assetType)
                    .Select(x => x.DependencyFile)
                    .ToList();

            return _runtimeMinifier.GetAssetPaths(assetType, attributes);
        }
    }
}
