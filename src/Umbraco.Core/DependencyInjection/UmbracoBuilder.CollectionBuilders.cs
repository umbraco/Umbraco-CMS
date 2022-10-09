using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Sections;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
///     Contains extensions methods for <see cref="IUmbracoBuilder" /> used for registering content apps.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Register a component.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder AddComponent<T>(this IUmbracoBuilder builder)
        where T : class, IComponent
    {
        builder.Components().Append<T>();
        return builder;
    }

    /// <summary>
    ///     Register a content app.
    /// </summary>
    /// <typeparam name="T">The type of the content app.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder AddContentApp<T>(this IUmbracoBuilder builder)
        where T : class, IContentAppFactory
    {
        builder.ContentApps().Append<T>();
        return builder;
    }

    /// <summary>
    ///     Register a content finder.
    /// </summary>
    /// <typeparam name="T">The type of the content finder.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder AddContentFinder<T>(this IUmbracoBuilder builder)
        where T : class, IContentFinder
    {
        builder.ContentFinders().Append<T>();
        return builder;
    }

    /// <summary>
    ///     Register a dashboard.
    /// </summary>
    /// <typeparam name="T">The type of the dashboard.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder AddDashboard<T>(this IUmbracoBuilder builder)
        where T : class, IDashboard
    {
        builder.Dashboards().Add<T>();
        return builder;
    }

    /// <summary>
    ///     Register a media url provider.
    /// </summary>
    /// <typeparam name="T">The type of the media url provider.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder AddMediaUrlProvider<T>(this IUmbracoBuilder builder)
        where T : class, IMediaUrlProvider
    {
        builder.MediaUrlProviders().Append<T>();
        return builder;
    }

    /// <summary>
    ///     Register a embed provider.
    /// </summary>
    /// <typeparam name="T">The type of the embed provider.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder AddEmbedProvider<T>(this IUmbracoBuilder builder)
        where T : class, IEmbedProvider
    {
        builder.EmbedProviders().Append<T>();
        return builder;
    }

    [Obsolete("Use AddEmbedProvider instead. This will be removed in Umbraco 11")]
    public static IUmbracoBuilder AddOEmbedProvider<T>(this IUmbracoBuilder builder)
        where T : class, IEmbedProvider => AddEmbedProvider<T>(builder);

    /// <summary>
    ///     Register a section.
    /// </summary>
    /// <typeparam name="T">The type of the section.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder AddSection<T>(this IUmbracoBuilder builder)
        where T : class, ISection
    {
        builder.Sections().Append<T>();
        return builder;
    }

    /// <summary>
    ///     Register a url provider.
    /// </summary>
    /// <typeparam name="T">The type of the url provider.</typeparam>
    /// <param name="builder">The Builder.</param>
    public static IUmbracoBuilder AddUrlProvider<T>(this IUmbracoBuilder builder)
        where T : class, IUrlProvider
    {
        builder.UrlProviders().Append<T>();
        return builder;
    }
}
