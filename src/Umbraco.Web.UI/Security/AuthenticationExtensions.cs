using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.Security
{
    public static class AuthenticationExtensions
    {
        public static IUmbracoBuilder AddExternalAuth(this IUmbracoBuilder builder)
        {
            builder.Services.ConfigureOptions<GoogleBackOfficeExternalLoginProviderOptions>();
            builder.Services.ConfigureOptions<GoogleMemberExternalLoginProviderOptions>();
            builder.Services.ConfigureOptions<TwitterMemberExternalLoginProviderOptions>();

            builder.AddBackOfficeExternalLogins(logins =>
            {
                logins.AddBackOfficeLogin(backOfficeAuthenticationBuilder =>
                {
                    backOfficeAuthenticationBuilder.AddGoogle(

                        // The scheme must be set with this method to work for the back office
                        backOfficeAuthenticationBuilder.SchemeForBackOffice(GoogleBackOfficeExternalLoginProviderOptions.SchemeName),
                        options =>
                        {
                            //  By default this is '/signin-google' but it needs to be changed to this
                            options.CallbackPath = "/umbraco-google-signin";
                            options.ClientId = "556128867795-qgjbaij0iv2iumgrd20j2s0ulbs31mvp.apps.googleusercontent.com";
                            options.ClientSecret = "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";
                        });
                });
            });

            builder.AddMemberExternalLogins(logins =>
            {
                logins.AddMemberLogin(
                    memberAuthenticationBuilder =>
                    {
                        memberAuthenticationBuilder.AddGoogle(

                            // The scheme must be set with this method to work for the back office
                            memberAuthenticationBuilder.SchemeForMembers(GoogleMemberExternalLoginProviderOptions.SchemeName),
                            options =>
                            {
                                options.ClientId = "556128867795-je49299kdvhdmp0oef1jpus9knputcdo.apps.googleusercontent.com";
                                options.ClientSecret = "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";
                            });
                    });
            });
            return builder;
        }
    }
}
