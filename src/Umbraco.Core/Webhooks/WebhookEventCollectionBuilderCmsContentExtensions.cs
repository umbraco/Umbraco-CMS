using Umbraco.Cms.Core.Webhooks.Events.Content;
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
    public static WebhookEventCollectionBuilderCmsContent AddDefault(this WebhookEventCollectionBuilderCmsContent builder)
    {
        builder.Builder
           .Append<ContentCopiedWebhookEvent>()
           .Append<ContentDeletedWebhookEvent>()
           .Append<ContentEmptiedRecycleBinWebhookEvent>()
           .Append<ContentMovedToRecycleBinWebhookEvent>()
           .Append<ContentMovedWebhookEvent>()
           .Append<ContentPublishedWebhookEvent>()
           .Append<ContentSavedWebhookEvent>()
           .Append<ContentSortedWebhookEvent>()
           .Append<ContentUnpublishedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the content blueprint events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContent AddBlueprint(this WebhookEventCollectionBuilderCmsContent builder)
    {
        builder.Builder
           .Append<ContentDeletedBlueprintWebhookEvent>()
           .Append<ContentSavedBlueprintWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the content version events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContent AddVersion(this WebhookEventCollectionBuilderCmsContent builder)
    {
        builder.Builder
           .Append<ContentDeletedVersionsWebhookEvent>()
           .Append<ContentRolledBackWebhookEvent>();

        return builder;
    }
}
