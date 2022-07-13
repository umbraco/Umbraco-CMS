using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    ///     Try to get the basic auth username and password from the http context.
    /// </summary>
    public static bool TryGetBasicAuthCredentials(this HttpContext httpContext, out string? username, out string? password)
    {
        username = null;
        password = null;

        if (httpContext.Request.Headers.TryGetValue("Authorization", out StringValues authHeaders))
        {
            var authHeader = authHeaders.ToString();
            if (authHeader is not null && authHeader.StartsWith("Basic"))
            {
                // Extract credentials.
                var encodedUsernamePassword = authHeader.Substring(6).Trim();
                Encoding encoding = Encoding.UTF8;
                var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                var seperatorIndex = usernamePassword.IndexOf(':');

                username = usernamePassword.Substring(0, seperatorIndex);
                password = usernamePassword.Substring(seperatorIndex + 1);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Runs the authentication process
    /// </summary>
    public static async Task<AuthenticateResult> AuthenticateBackOfficeAsync(this HttpContext? httpContext)
    {
        if (httpContext == null)
        {
            return AuthenticateResult.NoResult();
        }

        AuthenticateResult result =
            await httpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);

        if (!result.Succeeded)
        {
            result =
                await httpContext.AuthenticateAsync(Constants.Security.BackOfficeExternalAuthenticationType);
        }

        return result;
    }

    /// <summary>
    ///     Get the value in the request form or query string for the key
    /// </summary>
    public static string GetRequestValue(this HttpContext context, string key)
    {
        HttpRequest request = context.Request;
        if (!request.HasFormContentType)
        {
            return request.Query[key];
        }

        string value = request.Form[key];
        return value ?? request.Query[key];
    }

    public static void SetPrincipalForRequest(this HttpContext context, ClaimsPrincipal? principal)
    {
        if (principal is not null)
        {
            context.User = principal;
        }
    }

    public static void SetReasonPhrase(this HttpContext httpContext, string? reasonPhrase)
    {
        // TODO we should update this behavior, as HTTP2 do not have ReasonPhrase. Could as well be returned in body
        // https://github.com/aspnet/HttpAbstractions/issues/395
        IHttpResponseFeature? httpResponseFeature = httpContext.Features.Get<IHttpResponseFeature>();
        if (!(httpResponseFeature is null))
        {
            httpResponseFeature.ReasonPhrase = reasonPhrase;
        }
    }

    /// <summary>
    ///     This will return the current back office identity.
    /// </summary>
    /// <param name="http"></param>
    /// <returns>
    ///     Returns the current back office identity if an admin is authenticated otherwise null
    /// </returns>
    public static ClaimsIdentity? GetCurrentIdentity(this HttpContext http)
    {
        // If it's already a UmbracoBackOfficeIdentity
        ClaimsIdentity? backOfficeIdentity = http.User.GetUmbracoIdentity();
        if (backOfficeIdentity != null)
        {
            return backOfficeIdentity;
        }

        return null;
    }
}
