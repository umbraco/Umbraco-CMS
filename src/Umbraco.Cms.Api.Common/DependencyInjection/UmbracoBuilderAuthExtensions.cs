using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

public static class UmbracoBuilderAuthExtensions
{
    private static bool _initialized;

    public static IUmbracoBuilder AddUmbracoOpenIddict(this IUmbracoBuilder builder)
    {
        if (_initialized is false)
        {
            ConfigureOpenIddict(builder);
            _initialized = true;
        }

        return builder;
    }

    private static void ConfigureOpenIddict(IUmbracoBuilder builder)
    {
        builder.Services.AddOpenIddict()
            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the authorization and token endpoints.
                // - important: member endpoints MUST be added before backoffice endpoints to ensure that auto-discovery works for members
                // FIXME: swap paths here so member API is first (see comment above)
                options
                    .SetAuthorizationEndpointUris(
                        Paths.MemberApi.AuthorizationEndpoint.TrimStart(Constants.CharArrays.ForwardSlash))
                    .SetTokenEndpointUris(
                        Paths.MemberApi.TokenEndpoint.TrimStart(Constants.CharArrays.ForwardSlash))
                    .SetLogoutEndpointUris(
                        Paths.MemberApi.LogoutEndpoint.TrimStart(Constants.CharArrays.ForwardSlash))
                    .SetRevocationEndpointUris(
                        Paths.MemberApi.RevokeEndpoint.TrimStart(Constants.CharArrays.ForwardSlash));

                // Enable authorization code flow with PKCE
                options
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowRefreshTokenFlow();

                // Register the ASP.NET Core host and configure for custom authentication endpoint.
                options
                    .UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough();

                // Enable reference tokens
                // - see https://documentation.openiddict.com/configuration/token-storage.html
                options
                    .UseReferenceAccessTokens()
                    .UseReferenceRefreshTokens();

                // Use ASP.NET Core Data Protection for tokens instead of JWT.
                // This is more secure, and has the added benefit of having a high throughput
                // but means that all servers (such as in a load balanced setup)
                // needs to use the same application name and key ring,
                // however this is already recommended for load balancing, so should be fine.
                // See https://documentation.openiddict.com/configuration/token-formats.html#switching-to-data-protection-tokens
                // and https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-7.0
                // for more information
                options.UseDataProtection();

                // Register encryption and signing credentials to protect tokens.
                // Note that for tokens generated/validated using ASP.NET Core Data Protection,
                // a separate key ring is used, distinct from the credentials discussed in
                // https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
                // More details can be found here: https://github.com/openiddict/openiddict-core/issues/1892#issuecomment-1737308506
                // "When using ASP.NET Core Data Protection to generate opaque tokens, the signing and encryption credentials
                // registered via Add*Key/Certificate() are not used". But since OpenIddict requires the registration of such,
                // we can generate random keys per instance without them taking effect.
                // - see also https://github.com/openiddict/openiddict-core/issues/1231
                options
                    .AddEncryptionKey(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32))) // generate a cryptographically secure random 256-bits key
                    .AddSigningKey(new RsaSecurityKey(RSA.Create(keySizeInBits: 2048))); // generate RSA key with recommended size of 2048-bits
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();

                // Enable token entry validation
                // - see https://documentation.openiddict.com/configuration/token-storage.html#enabling-token-entry-validation-at-the-api-level
                options.EnableTokenEntryValidation();

                // Use ASP.NET Core Data Protection for tokens instead of JWT. (see note in AddServer)
                options.UseDataProtection();
            });

        builder.Services.AddHostedService<OpenIddictCleanup>();
    }
}
