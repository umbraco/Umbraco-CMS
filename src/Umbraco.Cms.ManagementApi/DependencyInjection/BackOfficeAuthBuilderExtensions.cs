using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Middleware;
using Umbraco.Cms.ManagementApi.Security;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.New.Cms.Infrastructure.HostedServices;
using Umbraco.New.Cms.Infrastructure.Security;

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
        builder.Services.AddAuthorization(CreatePolicies);

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
                    .SetAuthorizationEndpointUris(Controllers.Security.Paths.BackOfficeApiAuthorizationEndpoint)
                    .SetTokenEndpointUris(Controllers.Security.Paths.BackOfficeApiTokenEndpoint);

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
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        builder.Services.AddTransient<IBackOfficeApplicationManager, BackOfficeApplicationManager>();
        builder.Services.AddSingleton<IClientSecretManager, ClientSecretManager>();
        builder.Services.AddSingleton<BackOfficeAuthorizationInitializationMiddleware>();

        builder.Services.AddHostedService<OpenIddictCleanup>();
        builder.Services.AddHostedService<DatabaseManager>();

        return builder;
    }

    // TODO: remove this once EF is implemented
    public class DatabaseManager : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseManager(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            DbContext context = scope.ServiceProvider.GetRequiredService<DbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            // TODO: add BackOfficeAuthorizationInitializationMiddleware before UseAuthorization (to make it run for unauthorized API requests) and remove this
            IBackOfficeApplicationManager backOfficeApplicationManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
            await backOfficeApplicationManager.EnsureBackOfficeApplicationAsync(new Uri("https://localhost:44331/"), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    // TODO: move this to an appropriate location and implement the policy scheme that should be used for the new management APIs
    private static void CreatePolicies(AuthorizationOptions options)
    {
        void AddPolicy(string policyName, string claimType, params string[] allowedClaimValues)
        {
            options.AddPolicy($"New{policyName}", policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
                policy.RequireClaim(claimType, allowedClaimValues);
            });
        }

        // NOTE: these are ONLY sample policies that allow us to test the new management APIs
        AddPolicy(AuthorizationPolicies.SectionAccessContent, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Content);
        AddPolicy(AuthorizationPolicies.SectionAccessForContentTree, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Content);
        AddPolicy(AuthorizationPolicies.SectionAccessForMediaTree, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Media);
        AddPolicy(AuthorizationPolicies.SectionAccessMedia, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Media);
        AddPolicy(AuthorizationPolicies.SectionAccessContentOrMedia, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Content, Constants.Applications.Media);
    }
}
