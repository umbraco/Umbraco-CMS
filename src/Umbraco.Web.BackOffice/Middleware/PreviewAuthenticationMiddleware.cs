using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Middleware
{
    /// <summary>
    /// Ensures that preview pages (front-end routed) are authenticated with the back office identity appended to the principal alongside any default authentication that takes place
    /// </summary>
    public class PreviewAuthenticationMiddleware : IMiddleware
    {
        /// <inheritdoc/>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            if (!request.IsClientSideRequest())
            {
                var isPreview = request.HasPreviewCookie()
                    && context.User != null
                    && !request.IsBackOfficeRequest();

                if (isPreview)
                {
                    var cookieOptions = context.RequestServices.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>()
                        .Get(Constants.Security.BackOfficeAuthenticationType);

                    if (cookieOptions == null)
                    {
                        throw new InvalidOperationException("No cookie options found with name " + Constants.Security.BackOfficeAuthenticationType);
                    }

                    // If we've gotten this far it means a preview cookie has been set and a front-end umbraco document request is executing.
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
                                // Ok, we've got a real ticket, now we can add this ticket's identity to the current
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
