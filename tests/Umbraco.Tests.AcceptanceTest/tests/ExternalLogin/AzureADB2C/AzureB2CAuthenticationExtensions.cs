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
            var b2CSettings = configuration
                .GetSection("AzureB2C")
                .Get<AzureB2CSettings>() ?? new AzureB2CSettings();

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
                        options.ClientId = b2CSettings.ClientId;
                        options.ClientSecret = b2CSettings.ClientSecret;
                        options.CallbackPath = "/umbraco-b2c-users-signin";
                        options.MetadataAddress =
                            $"https://{b2CSettings.Domain}/{b2CSettings.Tenant}/{b2CSettings.Policy}/v2.0/.well-known/openid-configuration";

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
