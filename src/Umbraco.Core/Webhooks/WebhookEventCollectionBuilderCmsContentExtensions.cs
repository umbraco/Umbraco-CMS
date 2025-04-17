using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderCmsExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCmsContent" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsContentExtensions
{
    /// <summary>
    /// Adds the content events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContent AddDefault(this WebhookEventCollectionBuilderCmsContent builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
                builder.Builder
                    .Add<ContentCopiedWebhookEvent>()
                    .Add<ContentDeletedWebhookEvent>()
                    .Add<ContentEmptiedRecycleBinWebhookEvent>()
                    .Add<ContentMovedToRecycleBinWebhookEvent>()
                    .Add<ContentMovedWebhookEvent>()
                    .Add<ExtendedContentPublishedWebhookEvent>()
                    .Add<ExtendedContentSavedWebhookEvent>()
                    .Add<ContentSortedWebhookEvent>()
                    .Add<ContentUnpublishedWebhookEvent>();
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<ContentCopiedWebhookEvent>()
                    .Add<ContentDeletedWebhookEvent>()
                    .Add<ContentEmptiedRecycleBinWebhookEvent>()
                    .Add<ContentMovedToRecycleBinWebhookEvent>()
                    .Add<ContentMovedWebhookEvent>()
                    .Add<ContentPublishedWebhookEvent>()
                    .Add<ContentSavedWebhookEvent>()
                    .Add<ContentSortedWebhookEvent>()
                    .Add<ContentUnpublishedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyContentCopiedWebhookEvent>()
                    .Add<LegacyContentDeletedWebhookEvent>()
                    .Add<LegacyContentEmptiedRecycleBinWebhookEvent>()
                    .Add<LegacyContentMovedToRecycleBinWebhookEvent>()
                    .Add<LegacyContentMovedWebhookEvent>()
                    .Add<LegacyContentPublishedWebhookEvent>()
                    .Add<LegacyContentSavedWebhookEvent>()
                    .Add<LegacyContentSortedWebhookEvent>()
                    .Add<LegacyContentUnpublishedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the content blueprint events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContent AddBlueprint(this WebhookEventCollectionBuilderCmsContent builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<ContentDeletedBlueprintWebhookEvent>()
                    .Add<ContentSavedBlueprintWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyContentDeletedBlueprintWebhookEvent>()
                    .Add<LegacyContentSavedBlueprintWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the content version events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContent AddVersion(this WebhookEventCollectionBuilderCmsContent builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<ContentDeletedVersionsWebhookEvent>()
                    .Add<ContentRolledBackWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyContentDeletedVersionsWebhookEvent>()
                    .Add<LegacyContentRolledBackWebhookEvent>();
                break;
        }

        return builder;
    }
}
