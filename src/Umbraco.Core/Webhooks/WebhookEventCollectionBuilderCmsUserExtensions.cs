using Umbraco.Cms.Core.Webhooks.Events;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderCmsExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCmsUser" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsUserExtensions
{
    /// <summary>
    /// Adds the user events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsUser AddDefault(this WebhookEventCollectionBuilderCmsUser builder)
    {
        builder.Builder
            .Add<UserDeletedWebhookEvent>()
            .Add<UserSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the user login events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsUser AddLogin(this WebhookEventCollectionBuilderCmsUser builder)
    {
        builder.Builder
            .Add<UserLockedWebhookEvent>()
            .Add<UserLoginFailedWebhookEvent>()
            .Add<UserLoginRequiresVerificationWebhookEvent>()
            .Add<UserLoginSuccessWebhookEvent>()
            .Add<UserLogoutSuccessWebhookEvent>()
            .Add<UserTwoFactorRequestedWebhookEvent>()
            .Add<UserUnlockedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the user password events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsUser AddPassword(this WebhookEventCollectionBuilderCmsUser builder)
    {
        builder.Builder
            .Add<UserForgotPasswordRequestedWebhookEvent>()
            .Add<UserPasswordChangedWebhookEvent>()
            .Add<UserPasswordResetWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the user group events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsUser AddGroup(this WebhookEventCollectionBuilderCmsUser builder)
    {
        builder.Builder
            .Add<AssignedUserGroupPermissionsWebhookEvent>()
            .Add<UserGroupDeletedWebhookEvent>()
            .Add<UserGroupSavedWebhookEvent>();

        return builder;
    }
}
