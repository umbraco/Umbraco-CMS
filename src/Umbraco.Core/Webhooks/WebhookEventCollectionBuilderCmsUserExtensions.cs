using Umbraco.Cms.Core.Webhooks;
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
    public static WebhookEventCollectionBuilderCmsUser AddDefault(this WebhookEventCollectionBuilderCmsUser builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<UserDeletedWebhookEvent>()
                    .Add<UserSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyUserDeletedWebhookEvent>()
                    .Add<LegacyUserSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the user login events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsUser AddLogin(this WebhookEventCollectionBuilderCmsUser builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<UserLockedWebhookEvent>()
                    .Add<UserLoginFailedWebhookEvent>()
                    .Add<UserLoginRequiresVerificationWebhookEvent>()
                    .Add<UserLoginSuccessWebhookEvent>()
                    .Add<UserLogoutSuccessWebhookEvent>()
                    .Add<UserTwoFactorRequestedWebhookEvent>()
                    .Add<UserUnlockedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyUserLockedWebhookEvent>()
                    .Add<LegacyUserLoginFailedWebhookEvent>()
                    .Add<LegacyUserLoginRequiresVerificationWebhookEvent>()
                    .Add<LegacyUserLoginSuccessWebhookEvent>()
                    .Add<LegacyUserLogoutSuccessWebhookEvent>()
                    .Add<LegacyUserTwoFactorRequestedWebhookEvent>()
                    .Add<LegacyUserUnlockedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the user password events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsUser AddPassword(this WebhookEventCollectionBuilderCmsUser builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<UserForgotPasswordRequestedWebhookEvent>()
                    .Add<UserPasswordChangedWebhookEvent>()
                    .Add<UserPasswordResetWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyUserForgotPasswordRequestedWebhookEvent>()
                    .Add<LegacyUserPasswordChangedWebhookEvent>()
                    .Add<LegacyUserPasswordResetWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the user group events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsUser AddGroup(this WebhookEventCollectionBuilderCmsUser builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<AssignedUserGroupPermissionsWebhookEvent>()
                    .Add<UserGroupDeletedWebhookEvent>()
                    .Add<UserGroupSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyAssignedUserGroupPermissionsWebhookEvent>()
                    .Add<LegacyUserGroupDeletedWebhookEvent>()
                    .Add<LegacyUserGroupSavedWebhookEvent>();
                break;
        }

        return builder;
    }
}
