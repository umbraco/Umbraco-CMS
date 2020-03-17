using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Assets;
using Umbraco.Core.Runtime;
using Umbraco.Web.Composing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.JavaScript
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
                    ? new JavascriptFile(x)
                    : (IAssetFile) new CssFile(x);
            }).ToList();


            return runtimeMinifier.GetAssetPaths(assetType, dependencies);;
        }
    }
}
