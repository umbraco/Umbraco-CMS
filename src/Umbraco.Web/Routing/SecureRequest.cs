using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Service to encapsulate if a request is executing under HTTPS
    /// </summary>
    public class SecureRequest : ISecureRequest
    {
        /// <summary>
        /// A singleton because this is a simple class and we don't need to allocate new ones each time
        /// </summary>
        internal static SecureRequest Instance
        {
            get { return _instance; }
        }

        private static readonly SecureRequest _instance = new SecureRequest();

        /// <summary>
        /// Get/set the https headers to check against
        /// </summary>
        //TODO: Make this configurable via IWebRoutingSection?
        public static IDictionary<string, string> KnownForwardedHttpsHeaders = new Dictionary<string, string>
        {
            { "HTTP_X_FORWARDED_PROTO", "https" }
        };        

        public bool IsSecure(HttpRequestBase httpRequest)
        {
            var scheme = GetScheme(httpRequest);
            return scheme == Uri.UriSchemeHttps;
        }

        /// <summary>
        /// Returns either https or http depending on the information in the request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string GetScheme(HttpRequestBase request)
        {
            return GetScheme(request, KnownForwardedHttpsHeaders);
        }

        /// <summary>
        /// Returns either https or http depending on the information in the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="knownForwardedHttpsHeaders"></param>
        /// <returns></returns>
        private string GetScheme(HttpRequestBase request, IDictionary<string, string> knownForwardedHttpsHeaders)
        {
            //if the request is tagged as secure then just return https, we don't need to iterate the headers. This will
            //be the default implementation when sites are not behind a load balancer.
            if (request.IsSecureConnection) return Uri.UriSchemeHttps;

            //it's not secure so lets check the headers
            var httpsForwardHeaderExists = false;
            if (request.Headers != null)
            {
                foreach (var header in knownForwardedHttpsHeaders)
                {
                    var found = request.Headers.ContainsKey(header.Key);
                    if (found)
                    {
                        httpsForwardHeaderExists = string.Equals(request.Headers[header.Key], header.Value, StringComparison.OrdinalIgnoreCase);
                        if (httpsForwardHeaderExists) break;
                    }
                }
            }

            return httpsForwardHeaderExists ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        }
    }
}