using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="HttpRequest" />
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    ///     Check if a preview cookie exist
    /// </summary>
    public static bool HasPreviewCookie(this HttpRequest request)
        => request.Cookies.TryGetValue(Constants.Web.PreviewCookieName, out var cookieVal) &&
           !cookieVal.IsNullOrWhiteSpace();

    /// <summary>
    ///     Returns true if the request is a back office request
    /// </summary>
    public static bool IsBackOfficeRequest(this HttpRequest request)
    {
        PathString absPath = request.Path;
        UmbracoRequestPaths? umbReqPaths = request.HttpContext.RequestServices.GetService<UmbracoRequestPaths>();
        return umbReqPaths?.IsBackOfficeRequest(absPath) ?? false;
    }

    /// <summary>
    ///     Returns true if the request is for a client side extension
    /// </summary>
    public static bool IsClientSideRequest(this HttpRequest request)
    {
        PathString absPath = request.Path;
        UmbracoRequestPaths? umbReqPaths = request.HttpContext.RequestServices.GetService<UmbracoRequestPaths>();
        return umbReqPaths?.IsClientSideRequest(absPath) ?? false;
    }

    public static string? ClientCulture(this HttpRequest request)
        => request.Headers.TryGetValue("X-UMB-CULTURE", out StringValues values) ? values[0] : null;

    /// <summary>
    ///     Determines if a request is local.
    /// </summary>
    /// <returns>True if request is local</returns>
    /// <remarks>
    ///     Hat-tip: https://stackoverflow.com/a/41242493/489433
    /// </remarks>
    public static bool IsLocal(this HttpRequest request)
    {
        ConnectionInfo connection = request.HttpContext.Connection;
        if (connection.RemoteIpAddress?.IsSet() ?? false)
        {
            // We have a remote address set up
            return connection.LocalIpAddress?.IsSet() ?? false

                // Is local is same as remote, then we are local
                ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)

                // else we are remote if the remote IP address is not a loopback address
                : IPAddress.IsLoopback(connection.RemoteIpAddress);
        }

        return true;
    }

    public static string GetRawBodyString(this HttpRequest request, Encoding? encoding = null)
    {
        if (request.Body.CanSeek)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
        }

        using (var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8, leaveOpen: true))
        {
            var result = reader.ReadToEnd();
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
            }

            return result;
        }
    }

    public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding? encoding = null)
    {
        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
        }

        request.Body.Seek(0, SeekOrigin.Begin);

        using (var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8, leaveOpen: true))
        {
            var result = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);

            return result;
        }
    }

    /// <summary>
    ///     Gets the application URI, will use the one specified in settings if present
    /// </summary>
    public static Uri GetApplicationUri(this HttpRequest request, WebRoutingSettings routingSettings)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (routingSettings == null)
        {
            throw new ArgumentNullException(nameof(routingSettings));
        }

        if (string.IsNullOrEmpty(routingSettings.UmbracoApplicationUrl))
        {
            var requestUri = new Uri(request.GetDisplayUrl());

            // Create a new URI with the relative uri as /, this ensures that only the base path is returned.
            return new Uri(requestUri, "/");
        }

        return new Uri(routingSettings.UmbracoApplicationUrl);
    }

    /// <summary>
    ///     Gets the Umbraco `ufprt` encrypted string from the current request
    /// </summary>
    /// <param name="request">The current request</param>
    /// <returns>The extracted `ufprt` token.</returns>
    public static string? GetUfprt(this HttpRequest request)
    {
        if (request.HasFormContentType && request.Form.TryGetValue("ufprt", out StringValues formVal) &&
            formVal != StringValues.Empty)
        {
            return formVal.ToString();
        }

        if (request.Query.TryGetValue("ufprt", out StringValues queryVal) && queryVal != StringValues.Empty)
        {
            return queryVal.ToString();
        }

        return null;
    }

    private static bool IsSet(this IPAddress address)
    {
        const string nullIpAddress = "::1";
        return address.ToString() != nullIpAddress;
    }
}
