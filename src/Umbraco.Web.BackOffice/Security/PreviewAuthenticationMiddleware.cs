using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Ensures that preview pages (front-end routed) are authenticated with the back office identity appended to the principal alongside any default authentication that takes place
    /// </summary>
    public class PreviewAuthenticationMiddleware : IMiddleware
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public PreviewAuthenticationMiddleware(
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment)
        {
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            if (!request.IsClientSideRequest())
            {
                var isPreview = request.HasPreviewCookie()
                    && context.User != null
                    && !request.IsBackOfficeRequest(_globalSettings, _hostingEnvironment);

                if (isPreview)
                {
                    var cookieOptions = context.RequestServices.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>()
                        .Get(Constants.Security.BackOfficeAuthenticationType);

                    if (cookieOptions == null)
                        throw new InvalidOperationException("No cookie options found with name " + Constants.Security.BackOfficeAuthenticationType);

                    //If we've gotten this far it means a preview cookie has been set and a front-end umbraco document request is executing.
                    // In this case, authentication will not have occurred for an Umbraco back office User, however we need to perform the authentication
                    // for the user here so that the preview capability can be authorized otherwise only the non-preview page will be rendered.
                    if (request.Cookies.TryGetValue(cookieOptions.Cookie.Name, out var cookie))
                    {
                        var unprotected = cookieOptions.TicketDataFormat.Unprotect(cookie);
                        if (unprotected != null)
                        {
                            var backOfficeIdentity = unprotected.Principal.GetUmbracoIdentity();
                            if (backOfficeIdentity != null)
                            {
                                //Ok, we've got a real ticket, now we can add this ticket's identity to the current
                                // Principal, this means we'll have 2 identities assigned to the principal which we can
                                // use to authorize the preview and allow for a back office User.

                                context.User.AddIdentity(backOfficeIdentity);
                            }
                        }
                    }

                }
            }

            await next(context);
        }
    }
}
