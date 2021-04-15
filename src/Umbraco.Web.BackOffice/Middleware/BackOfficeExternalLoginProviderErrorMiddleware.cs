using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using HttpRequestExtensions = Umbraco.Extensions.HttpRequestExtensions;

namespace Umbraco.Cms.Web.BackOffice.Middleware
{
    /// <summary>
    /// Used to handle errors registered by external login providers
    /// </summary>
    /// <remarks>
    /// When an external login provider registers an error with <see cref="Extensions.HttpContextExtensions.SetExternalLoginProviderErrors"/> during the OAuth process,
    /// this middleware will detect that, store the errors into cookie data and redirect to the back office login so we can read the errors back out.
    /// </remarks>
    public class BackOfficeExternalLoginProviderErrorMiddleware : IMiddleware, IConfigureOptions<UmbracoPipelineOptions>
    {
        /// <summary>
        /// Adds ourselves to the Umbraco middleware pipeline before any endpoint routes are declared
        /// </summary>
        /// <param name="options"></param>
        public void Configure(UmbracoPipelineOptions options)
            => options.AddFilter(new UmbracoPipelineFilter(
                nameof(BackOfficeExternalLoginProviderErrorMiddleware),
                null,
                app => app.UseMiddleware<BackOfficeExternalLoginProviderErrorMiddleware>(),
                null));

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var shortCircuit = false;
            if (!HttpRequestExtensions.IsClientSideRequest(context.Request))
            {
                // check if we have any errors registered
                var errors = context.GetExternalLoginProviderErrors();
                if (errors != null)
                {
                    shortCircuit = true;

                    var serialized = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errors)));

                    context.Response.Cookies.Append(ViewDataExtensions.TokenExternalSignInError, serialized, new CookieOptions
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
}
