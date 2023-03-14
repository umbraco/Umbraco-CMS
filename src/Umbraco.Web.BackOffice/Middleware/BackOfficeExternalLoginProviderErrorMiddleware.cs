using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Middleware;

/// <summary>
///     Used to handle errors registered by external login providers
/// </summary>
/// <remarks>
///     When an external login provider registers an error with
///     <see cref="Extensions.HttpContextExtensions.SetExternalLoginProviderErrors" /> during the OAuth process,
///     this middleware will detect that, store the errors into cookie data and redirect to the back office login so we can
///     read the errors back out.
/// </remarks>
public class BackOfficeExternalLoginProviderErrorMiddleware : IMiddleware
{
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

                var serialized = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errors)));

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
