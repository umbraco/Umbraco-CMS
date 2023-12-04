using Umbraco.Cms.Core.Webhooks.Events.Template;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderCmsExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCmsTemplate" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsTemplateExtensions
{
    /// <summary>
    /// Adds the template webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsTemplate AddDefault(this WebhookEventCollectionBuilderCmsTemplate builder)
    {
        builder.Builder
            .Append<TemplateDeletedWebhookEvent>()
            .Append<TemplateSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the partial view webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCmsTemplate AddPartialView(this WebhookEventCollectionBuilderCmsTemplate builder)
    {
        builder.Builder
            .Append<PartialViewDeletedWebhookEvent>()
            .Append<PartialViewSavedWebhookEvent>();

        return builder;
    }
}
