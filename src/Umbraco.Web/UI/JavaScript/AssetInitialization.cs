using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.Config;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.UI.JavaScript
{
    internal abstract class AssetInitialization
    {
        /// <summary>
        /// Get all dependencies declared on property editors
        /// </summary>
        /// <param name="cdfType"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected JArray ScanPropertyEditors(ClientDependencyType cdfType, HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            var cdfAttributes =
                PropertyEditorResolver.Current.PropertyEditors
                                      .SelectMany(x => x.GetType().GetCustomAttributes<PropertyEditorAssetAttribute>(false))
                                      .Where(x => x.AssetType == cdfType)
                                      .Select(x => x.DependencyFile)
                                      .ToList();

            string jsOut;
            string cssOut;
            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(cdfAttributes, new HashSet<IClientDependencyPath>(), out jsOut, out cssOut, httpContext);

            var toParse = cdfType == ClientDependencyType.Javascript ? jsOut : cssOut;

            var result = new JArray();
            //split the result by the delimiter and add to the array
            foreach (var u in toParse.Split(new[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries))
            {
                result.Add(u);
            }
            return result;
        }

        /// <summary>
        /// This will use CDF to optimize the asset file collection
        /// </summary>
        /// <param name="fileRefs"></param>
        /// <param name="cdfType"></param>
        /// <param name="httpContext"></param>
        /// <returns>
        /// Return the asset URLs that should be loaded, if the application is in debug mode then the URLs returned will be the same as the ones
        /// passed in with the CDF version query strings appended so cache busting works correctly.
        /// </returns>
        protected JArray OptimizeAssetCollection(JArray fileRefs, ClientDependencyType cdfType, HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");

            var depenencies = fileRefs.Select(x =>
            {
                var asString = x.ToString();
                if (asString.StartsWith("/") == false)
                {
                    //most declarations with be made relative to the /umbraco folder, so things like lib/blah/blah.js
                    // so we need to turn them into absolutes here
                    if (Uri.IsWellFormedUriString(asString, UriKind.Relative))
                    {
                        var absolute = new Uri(httpContext.Request.Url, asString);
                        return (IClientDependencyFile)new BasicFile(cdfType) { FilePath = absolute.AbsolutePath };
                    }
                }
                return cdfType == ClientDependencyType.Javascript
                    ? (IClientDependencyFile)new JavascriptFile(asString)
                    : (IClientDependencyFile)new CssFile(asString);
            }).Where(x => x != null).ToList();

            //Get the output string for these registrations which will be processed by CDF correctly to stagger the output based
            // on internal vs external resources. The output will be delimited based on our custom Umbraco.Web.UI.JavaScript.DependencyPathRenderer
            string jsOut;
            string cssOut;
            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(depenencies, new HashSet<IClientDependencyPath>(), out jsOut, out cssOut, httpContext);

            var urls = cdfType == ClientDependencyType.Javascript
                ? jsOut.Split(new string[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries)
                : cssOut.Split(new string[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries);

            var result = new JArray();
            foreach (var u in urls)
            {
                result.Add(u);
            }
            return result;
        }

    }
}