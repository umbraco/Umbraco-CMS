using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.Config;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.JavaScript
{
    internal abstract class AssetInitialization
    {
        protected IEnumerable<string> ScanPropertyEditors(ClientDependencyType assetType, HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            var attributes = Current.PropertyEditors
                    .SelectMany(x => x.GetType().GetCustomAttributes<PropertyEditorAssetAttribute>(false))
                    .Where(x => x.AssetType == assetType)
                    .Select(x => x.DependencyFile)
                    .ToList();

            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(attributes, new HashSet<IClientDependencyPath>(), out var scripts, out var stylesheets, httpContext);

            var toParse = assetType == ClientDependencyType.Javascript ? scripts : stylesheets;
            return toParse.Split(new[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static IEnumerable<string> OptimizeAssetCollection(IEnumerable<string> assets, ClientDependencyType assetType, HttpContextBase httpContext)
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
                    return new BasicFile(assetType) { FilePath = new Uri(requestUrl, x).AbsolutePath };
                }

                return assetType == ClientDependencyType.Javascript
                    ? new JavascriptFile(x)
                    : (IClientDependencyFile) new CssFile(x);
            }).ToList();

            // get the output string for these registrations which will be processed by CDF correctly to stagger the output based
            // on internal vs external resources. The output will be delimited based on our custom Umbraco.Web.UI.JavaScript.DependencyPathRenderer
            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(dependencies, new HashSet<IClientDependencyPath>(), out var scripts, out var stylesheets, httpContext);

            var urls = assetType == ClientDependencyType.Javascript
                ? scripts.Split(new[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries)
                : stylesheets.Split(new[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries);

            return urls;
        }
    }
}
