using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

public static class BackOfficeAuthBuilderExtensions
{
    public static IUmbracoBuilder AddBackOfficeAuthentication(this IUmbracoBuilder builder)
    {
        builder
            .AddAuthentication()
            .AddUmbracoOpenIddict()
            .AddBackOfficeLogin();

        return builder;
    }

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

        builder.Services.AddScoped<BackOfficeSecurityStampValidator>();
        builder.Services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();
        builder.Services.ConfigureOptions<ConfigureBackOfficeSecurityStampValidatorOptions>();

        return builder;
    }
}

internal class BackofficePipelineFilter : UmbracoPipelineFilter
{
    public BackofficePipelineFilter(string name)
        : base(name)
        => PrePipeline = builder => builder.UseMiddleware<BackOfficeAuthorizationInitializationMiddleware>();
}
