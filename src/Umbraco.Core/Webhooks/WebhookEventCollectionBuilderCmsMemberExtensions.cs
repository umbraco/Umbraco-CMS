using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderCmsExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCmsMember" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsMemberExtensions
{
    /// <summary>
    /// Adds the member webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsMember AddDefault(this WebhookEventCollectionBuilderCmsMember builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<ExportedMemberWebhookEvent>()
                    .Add<MemberDeletedWebhookEvent>()
                    .Add<MemberSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyExportedMemberWebhookEvent>()
                    .Add<LegacyMemberDeletedWebhookEvent>()
                    .Add<LegacyMemberSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the member role webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsMember AddRoles(this WebhookEventCollectionBuilderCmsMember builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<AssignedMemberRolesWebhookEvent>()
                    .Add<RemovedMemberRolesWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyAssignedMemberRolesWebhookEvent>()
                    .Add<LegacyRemovedMemberRolesWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the member group webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsMember AddGroup(this WebhookEventCollectionBuilderCmsMember builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<MemberGroupDeletedWebhookEvent>()
                    .Add<MemberGroupSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyMemberGroupDeletedWebhookEvent>()
                    .Add<LegacyMemberGroupSavedWebhookEvent>();
                break;
        }

        return builder;
    }
}
