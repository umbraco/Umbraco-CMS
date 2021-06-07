using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Umbraco.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get the value in the request form or query string for the key
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

        public static void SetPrincipalForRequest(this HttpContext context, ClaimsPrincipal principal) => context.User = principal;

        public static void SetReasonPhrase(this HttpContext httpContext, string reasonPhrase)
        {
            // TODO: we should update this behavior, as HTTP2 do not have ReasonPhrase. Could as well be returned in body
            // https://github.com/aspnet/HttpAbstractions/issues/395
            IHttpResponseFeature httpResponseFeature = httpContext.Features.Get<IHttpResponseFeature>();
            if (!(httpResponseFeature is null))
            {
                httpResponseFeature.ReasonPhrase = reasonPhrase;
            }
        }

        /// <summary>
        /// This will return the current back office identity.
        /// </summary>
        /// <param name="http"></param>
        /// <returns>
        /// Returns the current back office identity if an admin is authenticated otherwise null
        /// </returns>
        public static ClaimsIdentity GetCurrentIdentity(this HttpContext http)
        {
            if (http == null)
            {
                throw new ArgumentNullException(nameof(http));
            }

            if (http.User == null)
            {
                return null; // there's no user at all so no identity
            }

            // If it's already a UmbracoBackOfficeIdentity
            ClaimsIdentity backOfficeIdentity = http.User.GetUmbracoIdentity();
            if (backOfficeIdentity != null)
            {
                return backOfficeIdentity;
            }

            return null;
        }

        public static void AddBackOfficeIdentityFromAuthenticationCookie(this HttpContext context)
        {
            CookieAuthenticationOptions cookieOptions = context.RequestServices.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>()
                .Get(Cms.Core.Constants.Security.BackOfficeAuthenticationType);

            if (cookieOptions == null)
            {
                throw new InvalidOperationException($"No cookie options found with name '{Cms.Core.Constants.Security.BackOfficeAuthenticationType}'");
            }

            if (context.Request.Cookies.TryGetValue(cookieOptions.Cookie.Name, out var cookie))
            {
                AuthenticationTicket unprotected = cookieOptions.TicketDataFormat.Unprotect(cookie);
                ClaimsIdentity backOfficeIdentity = unprotected?.Principal.GetUmbracoIdentity();
                if (backOfficeIdentity != null)
                {
                    // OK, we've got a real ticket, now we can add this ticket's identity to the current
                    // principal; this means we'll have 2 identities assigned to the principal which we can
                    // use for authorization and allow for a back office user.
                    context.User.AddIdentity(backOfficeIdentity);
                }
            }
        }
    }
}
