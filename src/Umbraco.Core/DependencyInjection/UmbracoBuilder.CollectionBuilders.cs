using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Contains extensions methods for <see cref="IUmbracoBuilder" />.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Register a component.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddComponent<T>(this IUmbracoBuilder builder)
        where T : IComponent
    {
        builder.Components().Append<T>();

        return builder;
    }

    /// <summary>
    /// Register a content app.
    /// </summary>
    /// <typeparam name="T">The type of the content app.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddContentApp<T>(this IUmbracoBuilder builder)
        where T : IContentAppFactory
    {
        builder.ContentApps().Append<T>();

        return builder;
    }

    /// <summary>
    /// Register a content finder.
    /// </summary>
    /// <typeparam name="T">The type of the content finder.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddContentFinder<T>(this IUmbracoBuilder builder)
        where T : IContentFinder
    {
        builder.ContentFinders().Append<T>();

        return builder;
    }

    /// <summary>
    /// Register a dashboard.
    /// </summary>
    /// <typeparam name="T">The type of the dashboard.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddDashboard<T>(this IUmbracoBuilder builder)
        where T : IDashboard
    {
        builder.Dashboards().Add<T>();

        return builder;
    }

    /// <summary>
    /// Register a manifest filter.
    /// </summary>
    /// <typeparam name="T">The type of the manifest filter.</typeparam>
    /// <param name="builder">The Builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddManifestFilter<T>(this IUmbracoBuilder builder)
        where T : IManifestFilter
    {
        builder.ManifestFilters().Append<T>();

        return builder;
    }

    /// <summary>
    /// Register a media URL provider.
    /// </summary>
    /// <typeparam name="T">The type of the media URL provider.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddMediaUrlProvider<T>(this IUmbracoBuilder builder)
        where T : IMediaUrlProvider
    {
        builder.MediaUrlProviders().Append<T>();

        return builder;
    }

    /// <summary>
    /// Register a embed provider.
    /// </summary>
    /// <typeparam name="T">The type of the embed provider.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddEmbedProvider<T>(this IUmbracoBuilder builder)
        where T : IEmbedProvider
    {
        builder.EmbedProviders().Append<T>();

        return builder;
    }

    /// <summary>
    /// Register a section.
    /// </summary>
    /// <typeparam name="T">The type of the section.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddSection<T>(this IUmbracoBuilder builder)
        where T : ISection
    {
        builder.Sections().Append<T>();

        return builder;
    }

    /// <summary>
    /// Register a URL provider.
    /// </summary>
    /// <typeparam name="T">The type of the URL provider.</typeparam>
    /// <param name="builder">The Builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddUrlProvider<T>(this IUmbracoBuilder builder)
        where T : IUrlProvider
    {
        builder.UrlProviders().Append<T>();

        return builder;
    }

    /// <summary>
    /// Add a map definition.
    /// </summary>
    /// <typeparam name="T">The type of map definition.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddMapDefinition<T>(this IUmbracoBuilder builder)
        where T : IMapDefinition
    {
        builder.MapDefinitions().Add<T>();

        return builder;
    }

    /// <summary>
    /// Add a webhook event.
    /// </summary>
    /// <typeparam name="T">The type of webhook event.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static IUmbracoBuilder AddWebhookEvent<T>(this IUmbracoBuilder builder)
        where T : IWebhookEvent
    {
        builder.WebhookEvents().Add<T>();

        return builder;
    }

    /// <summary>
    /// Add an IDynamicRootQueryStep to the DynamicRootQueryStepCollectionBuilder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IUmbracoBuilder AddDynamicRootStep<T>(this IUmbracoBuilder builder) where T : IDynamicRootQueryStep
    {
        builder.DynamicRootSteps().Append<T>();
        return builder;
    }
}
