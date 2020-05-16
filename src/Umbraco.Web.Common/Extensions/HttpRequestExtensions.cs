using System.Net;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Extensions
{
    public static class HttpRequestExtensions
    {
        internal static string ClientCulture(this HttpRequest request)
        {
            return request.Headers.TryGetValue("X-UMB-CULTURE", out var values) ? values[0] : null;
        }

        /// <summary>
        /// Determines if a request is local.
        /// </summary>
        /// <returns>True if request is local</returns>
        /// <remarks>
        /// Hat-tip: https://stackoverflow.com/a/41242493/489433
        /// </remarks>
        public static bool IsLocal(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress.IsSet())
            {
                // We have a remote address set up
                return connection.LocalIpAddress.IsSet()
                    // Is local is same as remote, then we are local
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    // else we are remote if the remote IP address is not a loopback address
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }

        private static bool IsSet(this IPAddress address)
        {
            const string NullIpAddress = "::1";
            return address != null && address.ToString() != NullIpAddress;
        }
    }
}
