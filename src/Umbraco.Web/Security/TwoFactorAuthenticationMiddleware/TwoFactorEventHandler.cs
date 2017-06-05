using System;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;

namespace Umbraco.Web.Security.TwoFactorAuthenticationMiddleware
{
    internal sealed class TwoFactorEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            UmbracoDefaultOwinStartup.MiddlewareConfigured += ConfigureTwoFactorAuthentication;
        }

        private void ConfigureTwoFactorAuthentication(object sender, OwinMiddlewareConfiguredEventArgs args)
        {
            var app = args.AppBuilder;
            var applicationContext = ApplicationContext.Current;

            app.SetUmbracoLoggerFactory();
            app.UseTwoFactorSignInCookie(Constants.Security.BackOfficeTwoFactorAuthenticationType, TimeSpan.FromMinutes(5));

            // We need to set these values again after our custom changes. Otherwise preview doesn't work.
            app.UseUmbracoBackOfficeCookieAuthentication(applicationContext)
                .UseUmbracoBackOfficeExternalCookieAuthentication(applicationContext)
                .UseUmbracoPreviewAuthentication(applicationContext);

            app.ConfigureUserManagerForUmbracoBackOffice<TwoFactorBackOfficeUserManager, BackOfficeIdentityUser>(
                applicationContext,
                (options, context) =>
                {
                    var membershipProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider();
                    var userManager = TwoFactorBackOfficeUserManager.Create(options,
                        applicationContext.Services.UserService,
                        applicationContext.Services.ExternalLoginService,
                        membershipProvider);
                    return userManager;
                });
        }
    }
}