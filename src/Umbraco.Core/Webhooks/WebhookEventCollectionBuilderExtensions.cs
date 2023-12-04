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
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddCms(this WebhookEventCollectionBuilder builder, bool onlyDefault = false)
        => builder.AddCms(builder =>
        {
            if (onlyDefault)
            {
                builder.AddDefault();
            }
            else
            {
                builder
                    .AddContent()
                    .AddContentType()
                    .AddDataType()
                    .AddDictionary()
                    .AddDomain()
                    .AddFile()
                    .AddHealthCheck()
                    .AddLanguage()
                    .AddMedia()
                    .AddMember()
                    .AddPackage()
                    .AddPublicAccess()
                    .AddRelation()
                    .AddRelationType()
                    .AddUser();
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
