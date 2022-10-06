using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class BackOfficeAuthBuilderExtensions
{
    public static IUmbracoBuilder AddBackOfficeAuthentication(this IUmbracoBuilder builder)
    {
        builder
            .AddDbContext()
            .AddOpenIddict();

        return builder;
    }

    private static IUmbracoBuilder AddDbContext(this IUmbracoBuilder builder)
    {
        builder.Services.AddDbContext<DbContext>(options =>
        {
            // Configure the DB context
            // TODO: use actual Umbraco DbContext once EF is implemented - and remove dependency on Microsoft.EntityFrameworkCore.InMemory
            options.UseInMemoryDatabase(nameof(DbContext));

            // Register the entity sets needed by OpenIddict.
            options.UseOpenIddict();
        });

        return builder;
    }

    private static IUmbracoBuilder AddOpenIddict(this IUmbracoBuilder builder)
    {
        builder.Services.AddAuthentication();

        builder.Services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                options
                    .UseEntityFrameworkCore()
                    .UseDbContext<DbContext>();
            })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the authorization and token endpoints.
                options
                    .SetAuthorizationEndpointUris("/umbraco/api/v1.0/back-office-authentication/authorize")
                    .SetTokenEndpointUris("/umbraco/api/v1.0/back-office-authentication/token");

                // Enable authorization code flow with PKCE
                options
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

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
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();

                // TODO: this is a workaround to make validated principals be perceived as explicit backoffice users by ClaimsPrincipalExtensions.GetUmbracoIdentity
                // we may not need it once cookie auth for backoffice is removed - validate and clean up if necessary
                options.Configure(validationOptions =>
                {
                    validationOptions.TokenValidationParameters.AuthenticationType = Constants.Security.BackOfficeAuthenticationType;
                });
            });

        builder.Services.AddHostedService<ClientIdManager>();

        return builder;
    }

    // TODO: move this somewhere (find an appropriate namespace for it)
    public class ClientIdManager : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientIdManager(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            DbContext context = scope.ServiceProvider.GetRequiredService<DbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            IOpenIddictApplicationManager manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            const string backofficeClientId = "umbraco-back-office";
            if (await manager.FindByClientIdAsync(backofficeClientId, cancellationToken) is null)
            {
                await manager.CreateAsync(
                    new OpenIddictApplicationDescriptor
                    {
                        ClientId = backofficeClientId,
                        // TODO: fix redirect URI + path
                        // how do we figure out the current backoffice host?
                        // - wait for first request?
                        // - use IServerAddressesFeature?
                        // - put it in config?
                        // should we support multiple callback URLS (for external apps)?
                        // check IHostingEnvironment.EnsureApplicationMainUrl
                        RedirectUris = { new Uri("https://localhost:44331/umbraco/login/callback/") },
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.ResponseTypes.Code
                        }
                    },
                    cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
