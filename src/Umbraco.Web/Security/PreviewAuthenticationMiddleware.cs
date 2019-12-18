using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{
    internal class PreviewAuthenticationMiddleware : OwinMiddleware
    {
        private readonly UmbracoBackOfficeCookieAuthOptions _cookieOptions;
        private readonly IGlobalSettings _globalSettings;

        /// <summary>
        /// Instantiates the middleware with an optional pointer to the next component.
        /// </summary>
        /// <param name="next"/>
        /// <param name="cookieOptions"></param>
        /// <param name="globalSettings"></param>
        public PreviewAuthenticationMiddleware(OwinMiddleware next,
            UmbracoBackOfficeCookieAuthOptions cookieOptions, IGlobalSettings globalSettings) : base(next)
        {
            _cookieOptions = cookieOptions;
            _globalSettings = globalSettings;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context"/>
        /// <returns/>
        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            if (request.Uri.IsClientSideRequest() == false)
            {
                var claimsPrincipal = context.Request.User as ClaimsPrincipal;
                var isPreview = request.HasPreviewCookie()
                    && claimsPrincipal != null
                    && request.Uri != null
                    && request.Uri.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath, _globalSettings) == false;
                if (isPreview)
                {
                    //If we've gotten this far it means a preview cookie has been set and a front-end umbraco document request is executing.
                    // In this case, authentication will not have occurred for an Umbraco back office User, however we need to perform the authentication
                    // for the user here so that the preview capability can be authorized otherwise only the non-preview page will be rendered.

                    var cookie = request.Cookies[_cookieOptions.CookieName];
                    if (cookie.IsNullOrWhiteSpace() == false)
                    {
                        var unprotected = _cookieOptions.TicketDataFormat.Unprotect(cookie);
                        if (unprotected != null)
                        {
                            //Ok, we've got a real ticket, now we can add this ticket's identity to the current
                            // Principal, this means we'll have 2 identities assigned to the principal which we can
                            // use to authorize the preview and allow for a back office User.
                            claimsPrincipal.AddIdentity(UmbracoBackOfficeIdentity.FromClaimsIdentity(unprotected.Identity));
                        }
                    }
                }
            }

            if (Next != null)
            {
                await Next.Invoke(context);
            }
        }
    }
}
