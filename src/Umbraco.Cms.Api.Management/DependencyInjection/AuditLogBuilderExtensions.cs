using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class AuditLogBuilderExtensions
{
    internal static IUmbracoBuilder AddAuditLogs(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IAuditLogPresentationFactory, AuditLogPresentationFactory>();
        builder.AddNotificationHandler<UserLoginSuccessNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserLogoutSuccessNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserLoginFailedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserForgotPasswordRequestedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserForgotPasswordChangedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserPasswordChangedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserPasswordResetNotification, BackOfficeUserManagerAuditer>();

        return builder;
    }
}
