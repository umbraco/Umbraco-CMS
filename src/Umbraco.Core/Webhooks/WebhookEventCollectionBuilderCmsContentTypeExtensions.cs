using Umbraco.Cms.Core.Webhooks.Events.ContentType;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderCmsExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCmsContentType" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsContentTypeExtensions
{
    /// <summary>
    /// Adds the document type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContentType AddDocumentType(this WebhookEventCollectionBuilderCmsContentType builder)
    {
        builder.Builder
            .Append<DocumentTypeChangedWebhookEvent>()
            .Append<DocumentTypeDeletedWebhookEvent>()
            .Append<DocumentTypeMovedWebhookEvent>()
            .Append<DocumentTypeSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the media type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContentType AddMediaType(this WebhookEventCollectionBuilderCmsContentType builder)
    {
        builder.Builder
            .Append<MediaTypeChangedWebhookEvent>()
            .Append<MediaTypeDeletedWebhookEvent>()
            .Append<MediaTypeMovedWebhookEvent>()
            .Append<MediaTypeSavedWebhookEvent>();

        return builder;
    }


    /// <summary>
    /// Adds the member type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContentType AddMemberType(this WebhookEventCollectionBuilderCmsContentType builder)
    {
        builder.Builder
            .Append<MemberTypeChangedWebhookEvent>()
            .Append<MemberTypeDeletedWebhookEvent>()
            .Append<MemberTypeMovedWebhookEvent>()
            .Append<MemberTypeSavedWebhookEvent>();

        return builder;
    }
}
