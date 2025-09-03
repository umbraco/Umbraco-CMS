using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.AcceptanceTest.ExternalLogin.AzureADB2C
{
    public static class AzureB2CAuthenticationExtensions
    {
        public static IUmbracoBuilder ConfigureAuthentication(this IUmbracoBuilder builder,
            IConfiguration configuration)
        {
            var b2cSettings = new AzureB2CSettings();

            builder.AddBackOfficeExternalLogins(logins =>
            {
                const string schemeName = AzureB2COptions.SchemeName;
                var backOfficeScheme = Constants.Security.BackOfficeExternalAuthenticationTypePrefix + schemeName;

                logins.AddBackOfficeLogin(backOfficeAuth =>
                {
                    backOfficeAuth.AddOpenIdConnect(backOfficeScheme, options =>
                    {
                        options.RequireHttpsMetadata = true;
                        options.SaveTokens = true;
                        options.ClientId = b2cSettings.ClientId;
                        options.ClientSecret = b2cSettings.ClientSecret;
                        options.CallbackPath = "/umbraco-b2c-users-signin";
                        options.MetadataAddress =
                            $"https://{b2cSettings.Domain}/{b2cSettings.Tenant}/{b2cSettings.Policy}/v2.0/.well-known/openid-configuration";

                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.TokenValidationParameters.SaveSigninToken = true;
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.TokenValidationParameters.NameClaimType = "name";
                        options.TokenValidationParameters.RoleClaimType = "role";

                        options.Events = new OpenIdConnectEvents
                        {
                            OnTokenResponseReceived = context =>
                            {
                                if (string.IsNullOrEmpty(context.TokenEndpointResponse.AccessToken))
                                {
                                    context.TokenEndpointResponse.AccessToken = "empty_access_token";
                                }

                                return Task.CompletedTask;
                            },

                            OnTokenValidated = context =>
                            {
                                var identity = context.Principal!.Identities.First();

                                var email = identity.FindFirst("emails")?.Value
                                            ?? identity.FindFirst(ClaimTypes.Email)?.Value;

                                if (!string.IsNullOrWhiteSpace(email))
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Email, email));
                                    identity.AddClaim(new Claim("email", email));
                                }

                                return Task.CompletedTask;
                            }
                        };
                    });
                });
            });

            return builder;
        }
    }
}
