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
        /// This will check if we're in release mode, if so it will create a CDF URL to load them all in at once
        /// </summary>
        /// <param name="fileRefs"></param>
        /// <param name="cdfType"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected JArray CheckIfReleaseAndOptimized(JArray fileRefs, ClientDependencyType cdfType, HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            
            if (httpContext.IsDebuggingEnabled == false)
            {
                return GetOptimized(fileRefs, cdfType, httpContext);
            }
            return fileRefs;
        }

        /// <summary>
        /// Return array of optimized URLs
        /// </summary>
        /// <param name="fileRefs"></param>
        /// <param name="cdfType"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected JArray GetOptimized(JArray fileRefs, ClientDependencyType cdfType, HttpContextBase httpContext)
        {
            var depenencies = fileRefs.Select(x =>
                {
                    var asString = x.ToString();
                    if (asString.StartsWith("/") == false)
                    {
                        if (Uri.IsWellFormedUriString(asString, UriKind.Relative))
                        {
                            var absolute = new Uri(httpContext.Request.Url, asString);
                            return new BasicFile(cdfType) { FilePath = absolute.AbsolutePath };
                        }
                        return null;
                    }
                    return new JavascriptFile(asString);
                }).Where(x => x != null);

            var urls = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                depenencies,
                cdfType,
                httpContext);

            var result = new JArray();
            foreach (var u in urls)
            {
                result.Add(u);
            }
            return result;
        }
    }
}