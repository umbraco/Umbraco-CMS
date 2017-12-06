using System;
using System.Web;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
    public interface ISecureRequest
    {
        bool IsSecure(HttpRequestBase httpRequest);
        string GetScheme(HttpRequestBase request);
    }

    public class SecureRequest : ISecureRequest
    {
        /// <summary>
        /// Get/set the https headers to check against
        /// </summary>
        //TODO: Make this configurable via IWebRoutingSection?
        public static string[] KnownForwardedHttpsHeaders = new[] { "HTTP_X_FORWARDED_PROTO" };

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
        private string GetScheme(HttpRequestBase request, string[] knownForwardedHttpsHeaders)
        {
            var httpsForwardHeaderExists = false;
            if (request.Headers != null)
            {
                foreach (var header in knownForwardedHttpsHeaders)
                {
                    httpsForwardHeaderExists = request.Headers.ContainsKey(header);
                    if (httpsForwardHeaderExists) break;
                }
            }

            return httpsForwardHeaderExists || request.IsSecureConnection ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        }
    }
}