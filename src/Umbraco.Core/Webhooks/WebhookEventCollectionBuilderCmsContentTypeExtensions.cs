using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events;
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
    public static WebhookEventCollectionBuilderCmsContentType AddDocumentType(this WebhookEventCollectionBuilderCmsContentType builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<DocumentTypeChangedWebhookEvent>()
                    .Add<DocumentTypeDeletedWebhookEvent>()
                    .Add<DocumentTypeMovedWebhookEvent>()
                    .Add<DocumentTypeSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyDocumentTypeChangedWebhookEvent>()
                    .Add<LegacyDocumentTypeDeletedWebhookEvent>()
                    .Add<LegacyDocumentTypeMovedWebhookEvent>()
                    .Add<LegacyDocumentTypeSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the media type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContentType AddMediaType(this WebhookEventCollectionBuilderCmsContentType builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<MediaTypeChangedWebhookEvent>()
                    .Add<MediaTypeDeletedWebhookEvent>()
                    .Add<MediaTypeMovedWebhookEvent>()
                    .Add<MediaTypeSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyMediaTypeChangedWebhookEvent>()
                    .Add<LegacyMediaTypeDeletedWebhookEvent>()
                    .Add<LegacyMediaTypeMovedWebhookEvent>()
                    .Add<LegacyMediaTypeSavedWebhookEvent>();
                break;
        }

        return builder;
    }


    /// <summary>
    /// Adds the member type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsContentType AddMemberType(this WebhookEventCollectionBuilderCmsContentType builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<MemberTypeChangedWebhookEvent>()
                    .Add<MemberTypeDeletedWebhookEvent>()
                    .Add<MemberTypeMovedWebhookEvent>()
                    .Add<MemberTypeSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyMemberTypeChangedWebhookEvent>()
                    .Add<LegacyMemberTypeDeletedWebhookEvent>()
                    .Add<LegacyMemberTypeMovedWebhookEvent>()
                    .Add<LegacyMemberTypeSavedWebhookEvent>();
                break;
        }

        return builder;
    }
}
