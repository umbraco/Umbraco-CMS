using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderCmsExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCmsFile" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsFileExtensions
{
    /// <summary>
    /// Adds the partial view webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsFile AddPartialView(this WebhookEventCollectionBuilderCmsFile builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<PartialViewDeletedWebhookEvent>()
                    .Add<PartialViewSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyPartialViewDeletedWebhookEvent>()
                    .Add<LegacyPartialViewSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the script webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsFile AddScript(this WebhookEventCollectionBuilderCmsFile builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<ScriptDeletedWebhookEvent>()
                    .Add<ScriptSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyScriptDeletedWebhookEvent>()
                    .Add<LegacyScriptSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the stylesheet webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsFile AddStylesheet(this WebhookEventCollectionBuilderCmsFile builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<StylesheetDeletedWebhookEvent>()
                    .Add<StylesheetSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyStylesheetDeletedWebhookEvent>()
                    .Add<LegacyStylesheetSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the template webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsFile AddTemplate(this WebhookEventCollectionBuilderCmsFile builder, WebhookPayloadType payloadType = WebhookPayloadType.Legacy)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<TemplateDeletedWebhookEvent>()
                    .Add<TemplateSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyTemplateDeletedWebhookEvent>()
                    .Add<LegacyTemplateSavedWebhookEvent>();
                break;
        }

        return builder;
    }
}
