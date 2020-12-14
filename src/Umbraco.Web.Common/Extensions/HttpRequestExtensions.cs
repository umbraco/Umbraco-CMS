using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;

namespace Umbraco.Extensions
{
    public static class HttpRequestExtensions
    {

        /// <summary>
        /// Check if a preview cookie exist
        /// </summary>
        public static bool HasPreviewCookie(this HttpRequest request)
            => request.Cookies.TryGetValue(Constants.Web.PreviewCookieName, out var cookieVal) && !cookieVal.IsNullOrWhiteSpace();

        public static bool IsBackOfficeRequest(this HttpRequest request, GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
            => new Uri(request.GetEncodedUrl(), UriKind.RelativeOrAbsolute).IsBackOfficeRequest(globalSettings, hostingEnvironment);

        public static bool IsClientSideRequest(this HttpRequest request)
            => new Uri(request.GetEncodedUrl(), UriKind.RelativeOrAbsolute).IsClientSideRequest();

        public static bool IsDefaultBackOfficeRequest(this HttpRequest request, GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
            => new Uri(request.GetEncodedUrl(), UriKind.RelativeOrAbsolute).IsDefaultBackOfficeRequest(globalSettings, hostingEnvironment);

        public static string ClientCulture(this HttpRequest request)
            => request.Headers.TryGetValue("X-UMB-CULTURE", out var values) ? values[0] : null;

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

        public static string GetRawBodyString(this HttpRequest request, Encoding encoding = null)
        {
            request.Body.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8, leaveOpen: true))
            {
                var result = reader.ReadToEnd();
                request.Body.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }

        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            request.Body.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8, leaveOpen: true))
            {
                var result = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }
    }
}
