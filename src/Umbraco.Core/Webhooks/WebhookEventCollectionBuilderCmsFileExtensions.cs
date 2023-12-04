using Umbraco.Cms.Core.Webhooks.Events.File;
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
    public static WebhookEventCollectionBuilderCmsFile AddPartialView(this WebhookEventCollectionBuilderCmsFile builder)
    {
        builder.Builder
            .Append<PartialViewDeletedWebhookEvent>()
            .Append<PartialViewSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the script webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsFile AddScript(this WebhookEventCollectionBuilderCmsFile builder)
    {
        builder.Builder
            .Append<ScriptDeletedWebhookEvent>()
            .Append<ScriptSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the stylesheet webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsFile AddStylesheet(this WebhookEventCollectionBuilderCmsFile builder)
    {
        builder.Builder
            .Append<StylesheetDeletedWebhookEvent>()
            .Append<StylesheetSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the template webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsFile AddTemplate(this WebhookEventCollectionBuilderCmsFile builder)
    {
        builder.Builder
            .Append<TemplateDeletedWebhookEvent>()
            .Append<TemplateSavedWebhookEvent>();

        return builder;
    }
}
