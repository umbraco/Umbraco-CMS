using Umbraco.Cms.Core.Webhooks.Events.User;
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
            .Append<UserDeletedWebhookEvent>()
            .Append<UserSavedWebhookEvent>();

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
            .Append<UserLockedWebhookEvent>()
            .Append<UserLoginFailedWebhookEvent>()
            .Append<UserLoginRequiresVerificationWebhookEvent>()
            .Append<UserLoginSuccessWebhookEvent>()
            .Append<UserLogoutSuccessWebhookEvent>()
            .Append<UserTwoFactorRequestedWebhookEvent>()
            .Append<UserUnlockedWebhookEvent>();

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
            .Append<UserForgotPasswordRequestedWebhookEvent>()
            .Append<UserPasswordChangedWebhookEvent>()
            .Append<UserPasswordResetWebhookEvent>();

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
            .Append<AssignedUserGroupPermissionsWebhookEvent>()
            .Append<UserGroupDeletedWebhookEvent>()
            .Append<UserGroupSavedWebhookEvent>();

        return builder;
    }
}
