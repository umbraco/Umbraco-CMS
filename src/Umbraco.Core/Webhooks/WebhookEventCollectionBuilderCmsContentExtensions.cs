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
    public static WebhookEventCollectionBuilderCmsContent AddDefault(this WebhookEventCollectionBuilderCmsContent builder)
    {
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
           .Add<ContentDeletedBlueprintWebhookEvent>()
           .Add<ContentSavedBlueprintWebhookEvent>();

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
           .Add<ContentDeletedVersionsWebhookEvent>()
           .Add<ContentRolledBackWebhookEvent>();

        return builder;
    }
}
