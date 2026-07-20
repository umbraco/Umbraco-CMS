using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Middleware;

/// <summary>
///     Used to handle errors registered by external login providers.
/// </summary>
/// <remarks>
///     When an external login provider registers an error with
///     <see cref="HttpContextExtensions.SetExternalLoginProviderErrors" /> during the OAuth process,
///     this middleware will detect that, store the errors into cookie data and redirect to the back office login so we can
///     read the errors back out.
/// </remarks>
public class BackOfficeExternalLoginProviderErrorMiddleware : IMiddleware
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeExternalLoginProviderErrorMiddleware"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used for serializing error responses.</param>
    public BackOfficeExternalLoginProviderErrorMiddleware(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// Processes HTTP requests to handle errors from external login providers during back office authentication.
    /// If such errors are detected, they are serialized, encoded, and stored in a secure, short-lived cookie, then the request is redirected to the original URL.
    /// If no errors are present, the request is passed to the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The current HTTP context for the request.</param>
    /// <param name="next">The delegate representing the next middleware in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation of the middleware.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var shortCircuit = false;
        if (!context.Request.IsClientSideRequest())
        {
            // check if we have any errors registered
            BackOfficeExternalLoginProviderErrors? errors = context.GetExternalLoginProviderErrors();
            if (errors != null)
            {
                shortCircuit = true;

                var serialized = Convert.ToBase64String(Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(errors)));

                context.Response.Cookies.Append(
                    ViewDataExtensions.TokenExternalSignInError,
                    serialized,
                    new CookieOptions
                    {
                        Expires = DateTime.Now.AddMinutes(5),
                        HttpOnly = true,
                        Secure = context.Request.IsHttps
                    });

                context.Response.Redirect(context.Request.GetEncodedUrl());
            }
        }

        if (next != null && !shortCircuit)
        {
            await next(context);
        }
    }
}
