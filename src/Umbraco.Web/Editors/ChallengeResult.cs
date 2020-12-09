using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors
{
    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri, string userId = null)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }

        private string LoginProvider { get; set; }
        private string RedirectUri { get; set; }
        private string UserId { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            //Ensure the forms auth module doesn't do a redirect!
            context.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;

            var owinCtx = context.HttpContext.GetOwinContext();

            //First, see if a custom challenge result callback is specified for the provider
            // and use it instead of the default if one is supplied.
            var loginProvider = owinCtx.Authentication
                .GetExternalAuthenticationTypes()
                .FirstOrDefault(p => p.AuthenticationType == LoginProvider);
            if (loginProvider != null)
            {
                var providerChallengeResult = loginProvider.GetSignInChallengeResult(owinCtx);
                if (providerChallengeResult != null)
                {
                    owinCtx.Authentication.Challenge(providerChallengeResult, LoginProvider);
                    return;
                }
            }

            var properties = new AuthenticationProperties() { RedirectUri = RedirectUri.EnsureEndsWith('/') };
            if (UserId != null)
            {
                properties.Dictionary[BackOfficeController.XsrfKey] = UserId;
            }
            owinCtx.Authentication.Challenge(properties, LoginProvider);
        }
    }

}
