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
    public static WebhookEventCollectionBuilderCmsMember AddDefault(this WebhookEventCollectionBuilderCmsMember builder)
    {
        builder.Builder
            .Add<ExportedMemberWebhookEvent>()
            .Add<MemberDeletedWebhookEvent>()
            .Add<MemberSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the member role webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsMember AddRoles(this WebhookEventCollectionBuilderCmsMember builder)
    {
        builder.Builder
            .Add<AssignedMemberRolesWebhookEvent>()
            .Add<RemovedMemberRolesWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the member group webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsMember AddGroup(this WebhookEventCollectionBuilderCmsMember builder)
    {
        builder.Builder
            .Add<MemberGroupDeletedWebhookEvent>()
            .Add<MemberGroupSavedWebhookEvent>();

        return builder;
    }
}
