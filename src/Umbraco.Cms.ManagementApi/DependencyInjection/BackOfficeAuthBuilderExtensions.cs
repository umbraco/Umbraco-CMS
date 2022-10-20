using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Middleware;
using Umbraco.Cms.ManagementApi.Security;
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

                // TODO: this is a workaround to make validated principals be perceived as explicit backoffice users by ClaimsPrincipalExtensions.GetUmbracoIdentity
                // we may not need it once cookie auth for backoffice is removed - validate and clean up if necessary
                options.Configure(validationOptions =>
                {
                    validationOptions.TokenValidationParameters.AuthenticationType = Core.Constants.Security.BackOfficeAuthenticationType;
                });
            });

        builder.Services.AddTransient<IBackOfficeApplicationManager, BackOfficeApplicationManager>();
        builder.Services.AddSingleton<IClientSecretManager, ClientSecretManager>();
        builder.Services.AddSingleton<BackOfficeAuthorizationInitializationMiddleware>();

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
}
