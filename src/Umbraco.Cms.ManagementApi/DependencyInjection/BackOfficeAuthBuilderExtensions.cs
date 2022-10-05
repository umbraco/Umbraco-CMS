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

                // Register the encryption credentials.
                options
                    // TODO: use an actual key, i.e. options.AddEncryptionKey(new SymmetricSecurityKey(..));
                    .AddDevelopmentEncryptionCertificate()
                    .DisableAccessTokenEncryption();

                // Register the signing credentials.
                options
                    // TODO: use an actual certificate here
                    .AddDevelopmentSigningCertificate();

                // // Register available scopes
                // // TODO: if we want to use scopes, we need to setup the available ones here
                // options
                //     .RegisterScopes("some_scope", "another_scope");

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

                options.Configure(validationOptions =>
                {
                    validationOptions.TokenValidationParameters.AuthenticationType = Constants.Security.BackOfficeAuthenticationType;
                });
            });

        builder.Services.AddHostedService<ClientIdManager>();

        return builder;
    }

    // TODO: if we want to use scopes instead of claims, this will come in handy!
    // install with builder.Services.AddTransient<IClaimsTransformation, ScopeClaimsTransformation>();
    // public class ScopeClaimsTransformation : IClaimsTransformation
    // {
    //     public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    //     {
    //         if (principal.HasClaim("scope") == false)
    //         {
    //             return Task.FromResult(principal);
    //         }
    //
    //         ImmutableArray<string> knownScopeClaims = principal.GetClaims("scope");
    //         var missingScopeClaims = knownScopeClaims.SelectMany(s => s.Split(' ')).Except(knownScopeClaims).ToArray();
    //         if (missingScopeClaims.Any() == false)
    //         {
    //             return Task.FromResult(principal);
    //         }
    //
    //         var claimsIdentity = new ClaimsIdentity();
    //         foreach (var missingScopeClaim in missingScopeClaims)
    //         {
    //             claimsIdentity.AddClaim("scope", missingScopeClaim);
    //         }
    //
    //         principal.AddIdentity(claimsIdentity);
    //         return Task.FromResult(principal);
    //     }
    // }

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
                            OpenIddictConstants.Permissions.ResponseTypes.Code,
                            // TODO: if we're going with scopes, we may need to add them here
                            // OpenIddictConstants.Permissions.Prefixes.Scope + "some_scope",
                            // OpenIddictConstants.Permissions.Prefixes.Scope + "another_scope"
                        }
                    },
                    cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
