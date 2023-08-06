using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Management.Middleware;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class BackOfficeAuthBuilderExtensions
{
    public static IUmbracoBuilder AddBackOfficeAuthentication(this IUmbracoBuilder builder)
    {
        builder
            .AddOpenIddict()
            .AddBackOfficeLogin();

        return builder;
    }

    private static IUmbracoBuilder AddOpenIddict(this IUmbracoBuilder builder)
    {
        builder.Services.AddAuthentication();
        builder.AddAuthorizationPolicies();

        builder.Services.AddOpenIddict()
            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the authorization and token endpoints.
                options
                    .SetAuthorizationEndpointUris(Controllers.Security.Paths.BackOfficeApiAuthorizationEndpoint.TrimStart('/'))
                    .SetTokenEndpointUris(Controllers.Security.Paths.BackOfficeApiTokenEndpoint.TrimStart('/'));

                // Enable authorization code flow with PKCE
                options
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowRefreshTokenFlow();

                // Register the encryption and signing credentials.
                // - see https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
                options
                    // TODO: use actual certificates here, see docs above
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate()
                    .DisableAccessTokenEncryption();

                // Register the ASP.NET Core host and configure for custom authentication endpoint.
                options
                    .UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough();

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

        builder.Services.AddTransient<IBackOfficeApplicationManager, BackOfficeApplicationManager>();
        builder.Services.AddSingleton<BackOfficeAuthorizationInitializationMiddleware>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new BackofficePipelineFilter("Backoffice")));

        builder.Services.AddHostedService<OpenIddictCleanup>();

        return builder;
    }

    private static IUmbracoBuilder AddBackOfficeLogin(this IUmbracoBuilder builder)
    {
        builder.Services
            .AddAuthentication()
            .AddCookie(Constants.Security.NewBackOfficeAuthenticationType, options =>
            {
                options.LoginPath = "/umbraco/login";
                options.Cookie.Name = Constants.Security.NewBackOfficeAuthenticationType;
            });

        return builder;
    }
}

internal class BackofficePipelineFilter : UmbracoPipelineFilter
{
    public BackofficePipelineFilter(string name)
        : base(name)
        => PrePipeline = builder => builder.UseMiddleware<BackOfficeAuthorizationInitializationMiddleware>();
}
