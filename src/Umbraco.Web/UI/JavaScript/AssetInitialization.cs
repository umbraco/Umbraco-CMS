using System;
using System.Linq;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.Config;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.UI.JavaScript
{
    internal abstract class AssetInitialization
    {
        /// <summary>
        /// This will check if we're in release mode, if so it will create a CDF URL to load them all in at once
        /// </summary>
        /// <param name="fileRefs"></param>
        /// <param name="cdfType"></param>
        /// <returns></returns>
        protected JArray CheckIfReleaseAndOptimized(JArray fileRefs, ClientDependencyType cdfType)
        {
            if (HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled == false)
            {
                return GetOptimized(fileRefs, cdfType);
            }
            return fileRefs;
        }

        /// <summary>
        /// Return array of optimized URLs
        /// </summary>
        /// <param name="fileRefs"></param>
        /// <param name="cdfType"></param>
        /// <returns></returns>
        protected JArray GetOptimized(JArray fileRefs, ClientDependencyType cdfType)
        {
            var depenencies = fileRefs.Select(x =>
                {
                    var asString = x.ToString();
                    if (asString.StartsWith("/") == false)
                    {
                        if (Uri.IsWellFormedUriString(asString, UriKind.Relative))
                        {
                            var absolute = new Uri(HttpContext.Current.Request.Url, asString);
                            return new BasicFile(cdfType) { FilePath = absolute.AbsolutePath };
                        }
                        return null;
                    }
                    return new JavascriptFile(asString);
                }).Where(x => x != null);

            var urls = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                depenencies,
                cdfType, 
                new HttpContextWrapper(HttpContext.Current));

            var result = new JArray();
            foreach (var u in urls)
            {
                result.Add(u);
            }
            return result;
        }
    }
}