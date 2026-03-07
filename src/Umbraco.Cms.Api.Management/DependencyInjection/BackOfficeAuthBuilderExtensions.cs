using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Management.Configuration;
using Umbraco.Cms.Api.Management.Handlers;
using Umbraco.Cms.Api.Management.Middleware;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring back office authentication services.
/// </summary>
public static class BackOfficeAuthBuilderExtensions
{
    /// <summary>
    /// Configures and adds the necessary authentication services for the Umbraco back office to the specified builder.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to which back office authentication services will be added.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> instance with back office authentication configured.</returns>
    public static IUmbracoBuilder AddBackOfficeAuthentication(this IUmbracoBuilder builder)
    {
        builder
            .AddAuthentication()
            .AddUmbracoOpenIddict()
            .AddBackOfficeLogin();

        return builder;
    }

    /// <summary>
    /// Registers handlers with the back-office authentication builder to automatically revoke user authentication tokens
    /// when certain user-related events occur, such as saving, deleting, or successful login of a user.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to configure with token revocation handlers.</param>
    /// <returns>The configured <see cref="IUmbracoBuilder"/> instance.</returns>
    public static IUmbracoBuilder AddTokenRevocation(this IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<UserSavedNotification, RevokeUserAuthenticationTokensNotificationHandler>();
        builder.AddNotificationAsyncHandler<UserDeletedNotification, RevokeUserAuthenticationTokensNotificationHandler>();
        builder.AddNotificationAsyncHandler<UserLoginSuccessNotification, RevokeUserAuthenticationTokensNotificationHandler>();

        return builder;
    }

    private static IUmbracoBuilder AddAuthentication(this IUmbracoBuilder builder)
    {
        builder.Services.AddAuthentication();
        builder.AddAuthorizationPolicies();

        builder.Services.AddTransient<IBackOfficeApplicationManager, BackOfficeApplicationManager>();
        builder.Services.AddSingleton<BackOfficeAuthorizationInitializationMiddleware>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new BackofficePipelineFilter("Backoffice")));

        return builder;
    }

    private static IUmbracoBuilder AddBackOfficeLogin(this IUmbracoBuilder builder)
    {
        builder.Services
            .AddAuthentication()

            // Add our custom schemes which are cookie handlers
            .AddCookie(Constants.Security.BackOfficeAuthenticationType)
            .AddCookie(Constants.Security.BackOfficeExternalAuthenticationType, o =>
            {
                o.Cookie.Name = Constants.Security.BackOfficeExternalAuthenticationType;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })

            // Add a cookie scheme that can be used for authenticating backoffice users outside the scope of the backoffice.
            .AddCookie(Constants.Security.BackOfficeExposedAuthenticationType, options =>
            {
                options.Cookie.Name = Constants.Security.BackOfficeExposedCookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = true;
            })

            // Although we don't natively support this, we add it anyways so that if end-users implement the required logic
            // they don't have to worry about manually adding this scheme or modifying the sign in manager
            .AddCookie(Constants.Security.BackOfficeTwoFactorAuthenticationType, options =>
            {
                options.Cookie.Name = Constants.Security.BackOfficeTwoFactorAuthenticationType;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddCookie(Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType, o =>
            {
                o.Cookie.Name = Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

        // Add OpnIddict server event handler to refresh the cookie that exposes the backoffice authentication outside the scope of the backoffice.
        builder.Services.AddSingleton<ExposeBackOfficeAuthenticationOpenIddictServerEventsHandler>();
        builder.Services.Configure<OpenIddictServerOptions>(options =>
        {
            options.Handlers.Add(
                OpenIddictServerHandlerDescriptor
                    .CreateBuilder<OpenIddictServerEvents.GenerateTokenContext>()
                    .UseSingletonHandler<ExposeBackOfficeAuthenticationOpenIddictServerEventsHandler>()
                    .Build());
            options.Handlers.Add(
                OpenIddictServerHandlerDescriptor
                    .CreateBuilder<OpenIddictServerEvents.ApplyRevocationResponseContext>()
                    .UseSingletonHandler<ExposeBackOfficeAuthenticationOpenIddictServerEventsHandler>()
                    .Build());
        });

        builder.Services.AddScoped<BackOfficeSecurityStampValidator>();
        builder.Services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();
        builder.Services.ConfigureOptions<ConfigureBackOfficeSecurityStampValidatorOptions>();

        return builder;
    }
}

internal sealed class BackofficePipelineFilter : UmbracoPipelineFilter
{
    /// <summary>Initializes a new instance of the <see cref="BackofficePipelineFilter"/> class.</summary>
    /// <param name="name">The name of the pipeline filter.</param>
    public BackofficePipelineFilter(string name)
        : base(name)
        => PrePipeline = builder => builder.UseMiddleware<BackOfficeAuthorizationInitializationMiddleware>();
}
