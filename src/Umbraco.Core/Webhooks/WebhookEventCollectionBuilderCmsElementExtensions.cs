using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderCmsExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCmsElement" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsElementExtensions
{
    /// <summary>
    /// Adds the element events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="payloadType">The webhook payload type.</param>
    public static WebhookEventCollectionBuilderCmsElement AddDefault(this WebhookEventCollectionBuilderCmsElement builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
                builder.Builder
                    .Add<ElementCopiedWebhookEvent>()
                    .Add<ElementDeletedWebhookEvent>()
                    .Add<ElementEmptiedRecycleBinWebhookEvent>()
                    .Add<ElementMovedToRecycleBinWebhookEvent>()
                    .Add<ElementMovedWebhookEvent>()
                    .Add<ExtendedElementPublishedWebhookEvent>()
                    .Add<ExtendedElementSavedWebhookEvent>()
                    .Add<ElementUnpublishedWebhookEvent>();
                break;
            case WebhookPayloadType.Minimal:
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<ElementCopiedWebhookEvent>()
                    .Add<ElementDeletedWebhookEvent>()
                    .Add<ElementEmptiedRecycleBinWebhookEvent>()
                    .Add<ElementMovedToRecycleBinWebhookEvent>()
                    .Add<ElementMovedWebhookEvent>()
                    .Add<ElementPublishedWebhookEvent>()
                    .Add<ElementSavedWebhookEvent>()
                    .Add<ElementUnpublishedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the element version events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="payloadType">The webhook payload type.</param>
    public static WebhookEventCollectionBuilderCmsElement AddVersion(this WebhookEventCollectionBuilderCmsElement builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        builder.Builder
            .Add<ElementDeletedVersionsWebhookEvent>()
            .Add<ElementRolledBackWebhookEvent>();

        return builder;
    }
}
