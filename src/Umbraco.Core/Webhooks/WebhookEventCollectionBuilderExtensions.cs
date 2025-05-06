using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilder" />.
/// </summary>
public static class WebhookEventCollectionBuilderExtensions
{
    /// <summary>
    /// Adds all available CMS webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="onlyDefault">If set to <c>true</c> only adds the default webhook events instead of all available.</param>
    /// <param name="payloadType">The configured payload type.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddCms(this WebhookEventCollectionBuilder builder, bool onlyDefault = false, WebhookPayloadType payloadType = Constants.Webhooks.DefaultPayloadType)
        => builder.AddCms(builder =>
        {
            if (onlyDefault)
            {
                builder.AddDefault(payloadType);
            }
            else
            {
                builder
                    .AddContent(onlyDefault, payloadType)
                    .AddContentType(payloadType)
                    .AddDataType(payloadType)
                    .AddDictionary(payloadType)
                    .AddDomain(payloadType)
                    .AddFile(payloadType)
                    .AddHealthCheck(payloadType)
                    .AddLanguage(payloadType)
                    .AddMedia(payloadType)
                    .AddMember(onlyDefault, payloadType)
                    .AddPackage(payloadType)
                    .AddPublicAccess(payloadType)
                    .AddRelation(payloadType)
                    .AddRelationType(payloadType)
                    .AddUser(onlyDefault, payloadType);
            }
        });

    /// <summary>
    /// Adds CMS webhook events specified in the <paramref name="cmsBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="cmsBuilder">The CMS builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddCms(this WebhookEventCollectionBuilder builder, Action<WebhookEventCollectionBuilderCms> cmsBuilder)
    {
        cmsBuilder(new WebhookEventCollectionBuilderCms(builder));

        return builder;
    }

    /// <summary>
    /// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS specific webhook events.
    /// </summary>
    public sealed class WebhookEventCollectionBuilderCms
    {
        internal WebhookEventCollectionBuilderCms(WebhookEventCollectionBuilder builder)
            => Builder = builder;

        internal WebhookEventCollectionBuilder Builder { get; }
    }
}
