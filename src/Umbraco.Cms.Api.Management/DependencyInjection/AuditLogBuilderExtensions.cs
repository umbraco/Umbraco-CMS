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
        builder.AddNotificationAsyncHandler<UserLoginSuccessNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationAsyncHandler<UserLogoutSuccessNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationAsyncHandler<UserLoginFailedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationAsyncHandler<UserForgotPasswordRequestedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationAsyncHandler<UserForgotPasswordChangedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationAsyncHandler<UserPasswordChangedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationAsyncHandler<UserPasswordResetNotification, BackOfficeUserManagerAuditer>();

        return builder;
    }
}
