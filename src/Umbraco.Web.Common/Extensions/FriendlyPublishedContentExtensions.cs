using System.Data;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Extensions;

public static class FriendlyPublishedContentExtensions
{
    private static IVariationContextAccessor VariationContextAccessor { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>();

    private static IDomainCache DomainCache { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IDomainCache>();

    private static IPublishedContentCache PublishedContentCache { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishedContentCache>();

    private static IPublishedMediaCache PublishedMediaCache { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishedMediaCache>();

    private static IDocumentNavigationQueryService DocumentNavigationQueryService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();

    private static IMediaNavigationQueryService MediaNavigationQueryService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>();

    private static IPublishedModelFactory PublishedModelFactory { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishedModelFactory>();

    private static IPublishedUrlProvider PublishedUrlProvider { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlProvider>();

    private static IUserService UserService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IUserService>();

    private static IUmbracoContextAccessor UmbracoContextAccessor { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>();

    private static ISiteDomainMapper SiteDomainHelper { get; } =
        StaticServiceProvider.Instance.GetRequiredService<ISiteDomainMapper>();

    private static IExamineManager ExamineManager { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IExamineManager>();

    private static IFileService FileService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IFileService>();

    private static IOptions<WebRoutingSettings> WebRoutingSettings { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IOptions<WebRoutingSettings>>();

    private static IContentTypeService ContentTypeService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>();

    private static IPublishedValueFallback PublishedValueFallback { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishedValueFallback>();

    private static IMediaTypeService MediaTypeService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IMediaTypeService>();

    private static IMemberTypeService MemberTypeService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>();

    private static IPublishStatusQueryService PublishStatusQueryService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>();

    private static INavigationQueryService GetNavigationQueryService(IPublishedContent content)
    {
        switch (content.ContentType.ItemType)
        {
            case PublishedItemType.Content:
                return DocumentNavigationQueryService;
            case PublishedItemType.Media:
                return MediaNavigationQueryService;
            default:
                throw new NotSupportedException("Unsupported content type.");
        }

    }

    private static IPublishedCache GetPublishedCache(IPublishedContent content)
    {
        switch (content.ContentType.ItemType)
        {
            case PublishedItemType.Content:
                return PublishedContentCache;
            case PublishedItemType.Media:
                return PublishedMediaCache;
            default:
                throw new NotSupportedException("Unsupported content type.");
        }
    }

    /// <summary>
    ///     Creates a strongly typed published content model for an internal published content.
    /// </summary>
    /// <param name="content">The internal published content.</param>
    /// <returns>The strongly typed published content model.</returns>
    public static IPublishedContent? CreateModel(
        this IPublishedContent content)
        => content.CreateModel(PublishedModelFactory);

    /// <summary>
    ///     Gets the name of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static string? Name(
        this IPublishedContent content,
        string? culture = null)
        => content.Name(VariationContextAccessor, culture);

    /// <summary>
    ///     Gets the URL segment of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">
    ///     The specific culture to get the URL segment for. If null is used the current culture is used
    ///     (Default is null).
    /// </param>
    public static string? UrlSegment(
        this IPublishedContent content,
        string? culture = null)
        => content.UrlSegment(VariationContextAccessor, culture);

    /// <summary>
    ///     Gets the culture date of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static DateTime CultureDate(
        this IPublishedContent content,
        string? culture = null)
        => content.CultureDate(VariationContextAccessor, culture);

    /// <summary>
    ///     Returns the current template Alias
    /// </summary>
    /// <returns>Empty string if none is set.</returns>
    public static string GetTemplateAlias(this IPublishedContent content)
        => content.GetTemplateAlias(FileService);

    public static bool IsAllowedTemplate(this IPublishedContent content, int templateId)
        => content.IsAllowedTemplate(ContentTypeService, WebRoutingSettings.Value, templateId);

    public static bool IsAllowedTemplate(this IPublishedContent content, string templateAlias)
        => content.IsAllowedTemplate(
            WebRoutingSettings.Value.DisableAlternativeTemplates,
            WebRoutingSettings.Value.ValidateAlternativeTemplates,
            templateAlias);

    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        bool disableAlternativeTemplates,
        bool validateAlternativeTemplates,
        int templateId)
        => content.IsAllowedTemplate(
            ContentTypeService,
            disableAlternativeTemplates,
            validateAlternativeTemplates,
            templateId);

    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        bool disableAlternativeTemplates,
        bool validateAlternativeTemplates,
        string templateAlias)
        => content.IsAllowedTemplate(
            FileService,
            ContentTypeService,
            disableAlternativeTemplates,
            validateAlternativeTemplates,
            templateAlias);

    /// <summary>
    ///     Gets a value indicating whether the content has a value for a property identified by its alias.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <returns>A value indicating whether the content has a value for the property identified by the alias.</returns>
    /// <remarks>Returns true if HasValue is true, or a fallback strategy can provide a value.</remarks>
    public static bool HasValue(
        this IPublishedContent content,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default)
        =>
            content.HasValue(PublishedValueFallback, alias, culture, segment, fallback);

    /// <summary>
    ///     Gets the value of a content's property identified by its alias, if it exists, otherwise a default value.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
    public static object? Value(this IPublishedContent content, string alias, string? culture = null, string? segment = null, Fallback fallback = default, object? defaultValue = default)
        => content.Value(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);

    /// <summary>
    ///     Gets the value of a content's property identified by its alias, converted to a specified type.
    /// </summary>
    /// <typeparam name="T">The target property type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
    public static T? Value<T>(this IPublishedContent content, string alias, string? culture = null, string? segment = null, Fallback fallback = default, T? defaultValue = default)
        => content.Value(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>
    ///     The root content (ancestor or self at level 1) for the specified <paramref name="content" />.
    /// </returns>
    /// <remarks>
    ///     This is the same as calling
    ///     <see cref="AncestorOrSelf(IPublishedContent, int)" /> with <c>maxLevel</c>
    ///     set to 1.
    /// </remarks>
    public static IPublishedContent Root(this IPublishedContent content)
        => content.Root(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService);

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" /> if it's of the
    ///     specified content type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>
    ///     The root content (ancestor or self at level 1) for the specified <paramref name="content" /> of content type
    ///     <typeparamref name="T" />.
    /// </returns>
    /// <remarks>
    ///     This is the same as calling
    ///     <see cref="AncestorOrSelf{T}(IPublishedContent, int)" /> with
    ///     <c>maxLevel</c> set to 1.
    /// </remarks>
    public static T? Root<T>(this IPublishedContent content)
        where T : class, IPublishedContent
        => content.Root<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService);

    /// <summary>
    ///     Gets the parent of the content item.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <typeparam name="T">The content type.</typeparam>
    /// <returns>The parent of content of the specified content type or <c>null</c>.</returns>
    public static T? Parent<T>(this IPublishedContent content)
        where T : class, IPublishedContent
        => content.Parent<T>(GetPublishedCache(content), GetNavigationQueryService(content));

    /// <summary>
    ///     Gets the parent of the content item.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The parent of content or <c>null</c>.</returns>
    public static IPublishedContent? Parent(this IPublishedContent content)
        => content.Parent<IPublishedContent>(GetPublishedCache(content), GetNavigationQueryService(content));

    /// <summary>
    ///     Gets the ancestors of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The ancestors of the content, in down-top order.</returns>
    /// <remarks>Does not consider the content itself.</remarks>
    public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content)
        => content.Ancestors(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService);

    /// <summary>
    ///     Gets the content and its ancestors.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The content and its ancestors, in down-top order.</returns>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content)
        => content.AncestorsOrSelf(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService);

    /// <summary>
    ///     Gets the content and its ancestors, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
    /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
    public static IEnumerable<T> AncestorsOrSelf<T>(this IPublishedContent content)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService);

    /// <summary>
    ///     Gets the ancestor of the content, i.e. its parent.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The ancestor of the content.</returns>
    public static IPublishedContent? Ancestor(this IPublishedContent content)
        => content.Ancestor(GetPublishedCache(content), GetNavigationQueryService(content));

    /// <summary>
    ///     Gets the nearest ancestor of the content, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    public static T? Ancestor<T>(this IPublishedContent content)
        where T : class, IPublishedContent
        => content.Ancestor<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService);

    /// <summary>
    ///     Gets the content or its nearest ancestor, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
    /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
    public static T? AncestorOrSelf<T>(this IPublishedContent content)
        where T : class, IPublishedContent
        => content.AncestorOrSelf<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService);

    /// <summary>
    ///     Returns all DescendantsOrSelf of all content referenced
    /// </summary>
    /// <param name="parentNodes"></param>
    /// <param name="docTypeAlias"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns></returns>
    /// <remarks>
    ///     This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
    /// </remarks>
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes, string docTypeAlias, string? culture = null)
        => parentNodes.DescendantsOrSelfOfType(VariationContextAccessor, GetPublishedCache(parentNodes.First()), GetNavigationQueryService(parentNodes.First()), PublishStatusQueryService, docTypeAlias, culture);

    /// <summary>
    ///     Returns all DescendantsOrSelf of all content referenced
    /// </summary>
    /// <param name="parentNodes"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns></returns>
    /// <remarks>
    ///     This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
    /// </remarks>
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        string? culture = null)
        where T : class, IPublishedContent
        => parentNodes.DescendantsOrSelf<T>(VariationContextAccessor, GetPublishedCache(parentNodes.First()), GetNavigationQueryService(parentNodes.First()), PublishStatusQueryService, culture);

    public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, string? culture = null)
        => content.Descendants(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, int level, string? culture = null)
        => content.Descendants(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantsOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    public static IEnumerable<T> Descendants<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static IEnumerable<T> Descendants<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        string? culture = null)
        => content.DescendantsOrSelf(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, int level, string? culture = null)
        => content.DescendantsOrSelf(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantsOrSelfOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    public static IPublishedContent? Descendant(this IPublishedContent content, string? culture = null)
        => content.Descendant(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static IPublishedContent? Descendant(this IPublishedContent content, int level, string? culture = null)
        => content.Descendant(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    public static IPublishedContent? DescendantOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    public static T? Descendant<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static T? Descendant<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    public static IPublishedContent DescendantOrSelf(this IPublishedContent content, string? culture = null)
        => content.DescendantOrSelf(VariationContextAccessor, PublishStatusQueryService, culture);

    public static IPublishedContent? DescendantOrSelf(this IPublishedContent content, int level, string? culture = null)
        => content.DescendantOrSelf(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    public static IPublishedContent? DescendantOrSelfOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantOrSelfOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    public static T? DescendantOrSelf<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static T? DescendantOrSelf<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, level, culture);

    /// <summary>
    ///     Gets the children of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">
    ///     The specific culture to get the URL children for. Default is null which will use the current culture in
    ///     <see cref="VariationContext" />
    /// </param>
    /// <remarks>
    ///     <para>Gets children that are available for the specified culture.</para>
    ///     <para>Children are sorted by their sortOrder.</para>
    ///     <para>
    ///         For culture,
    ///         if null is used the current culture is used.
    ///         If an empty string is used only invariant children are returned.
    ///         If "*" is used all children are returned.
    ///     </para>
    ///     <para>
    ///         If a variant culture is specified or there is a current culture in the <see cref="VariationContext" /> then the
    ///         Children returned
    ///         will include both the variant children matching the culture AND the invariant children because the invariant
    ///         children flow with the current culture.
    ///         However, if an empty string is specified only invariant children are returned.
    ///     </para>
    /// </remarks>
    public static IEnumerable<IPublishedContent> Children(this IPublishedContent content, string? culture = null)
        => content.Children(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    /// <summary>
    ///     Gets the children of the content, filtered by a predicate.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The children of the content, filtered by the predicate.</returns>
    /// <remarks>
    ///     <para>Children are sorted by their sortOrder.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent>? Children(
        this IPublishedContent content,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, predicate, culture);

    /// <summary>
    ///     Gets the children of the content, of any of the specified types.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The children of the content, of any of the specified types.</returns>
    public static IEnumerable<IPublishedContent>? ChildrenOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.ChildrenOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    /// <summary>
    ///     Gets the children of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The children of content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Children are sorted by their sortOrder.</para>
    /// </remarks>
    public static IEnumerable<T>? Children<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.Children<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static IPublishedContent? FirstChild(this IPublishedContent content, string? culture = null)
        => content.FirstChild(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    /// <summary>
    ///     Gets the first child of the content, of a given content type.
    /// </summary>
    public static IPublishedContent? FirstChildOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.FirstChildOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    public static IPublishedContent? FirstChild(this IPublishedContent content, Func<IPublishedContent, bool> predicate, string? culture = null)
        => content.FirstChild(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, predicate, culture);

    public static IPublishedContent? FirstChild(this IPublishedContent content, Guid uniqueId, string? culture = null)
        => content.FirstChild(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, uniqueId, culture);

    public static T? FirstChild<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    public static T? FirstChild<T>(this IPublishedContent content, Func<T, bool> predicate, string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, predicate, culture);

    /// <summary>
    ///     Gets the siblings of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent>? Siblings(this IPublishedContent content, string? culture = null)
        => content.Siblings(GetPublishedCache(content), GetNavigationQueryService(content), VariationContextAccessor, PublishStatusQueryService, culture);

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent>? SiblingsOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.SiblingsOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<T>? Siblings<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.Siblings<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content including the node itself.</returns>
    public static IEnumerable<IPublishedContent>? SiblingsAndSelf(
        this IPublishedContent content,
        string? culture = null)
        => content.SiblingsAndSelf(GetPublishedCache(content), GetNavigationQueryService(content), VariationContextAccessor, PublishStatusQueryService, culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    public static IEnumerable<IPublishedContent>? SiblingsAndSelfOfType(
        this IPublishedContent content,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsAndSelfOfType(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, contentTypeAlias, culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    public static IEnumerable<T>? SiblingsAndSelf<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.SiblingsAndSelf<T>(VariationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), PublishStatusQueryService, culture);

    /// <summary>
    ///     Gets the url of the content item.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the content item is a document, then this method returns the url of the
    ///         document. If it is a media, then this methods return the media url for the
    ///         'umbracoFile' property. Use the MediaUrl() method to get the media url for other
    ///         properties.
    ///     </para>
    ///     <para>
    ///         The value of this property is contextual. It depends on the 'current' request uri,
    ///         if any. In addition, when the content type is multi-lingual, this is the url for the
    ///         specified culture. Otherwise, it is the invariant url.
    ///     </para>
    /// </remarks>
    public static string Url(this IPublishedContent content, string? culture = null, UrlMode mode = UrlMode.Default)
        => content.Url(PublishedUrlProvider, culture, mode);

    /// <summary>
    ///     Gets the children of the content in a DataTable.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="contentTypeAliasFilter">An optional content type alias.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The children of the content.</returns>
    public static DataTable ChildrenAsTable(this IPublishedContent content, string contentTypeAliasFilter = "", string? culture = null)
        =>
            content.ChildrenAsTable(
                VariationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                ContentTypeService,
                MediaTypeService,
                MemberTypeService,
                PublishedUrlProvider,
                contentTypeAliasFilter,
                culture);

    /// <summary>
    ///     Gets the url for a media.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">The culture (use current culture by default).</param>
    /// <param name="mode">The url mode (use site configuration by default).</param>
    /// <param name="propertyAlias">The alias of the property (use 'umbracoFile' by default).</param>
    /// <returns>The url for the media.</returns>
    /// <remarks>
    ///     <para>
    ///         The value of this property is contextual. It depends on the 'current' request uri,
    ///         if any. In addition, when the content type is multi-lingual, this is the url for the
    ///         specified culture. Otherwise, it is the invariant url.
    ///     </para>
    /// </remarks>
    public static string MediaUrl(
        this IPublishedContent content,
        string? culture = null,
        UrlMode mode = UrlMode.Default,
        string propertyAlias = Constants.Conventions.Media.File)
        => content.MediaUrl(PublishedUrlProvider, culture, mode, propertyAlias);

    /// <summary>
    ///     Gets the name of the content item creator.
    /// </summary>
    /// <param name="content">The content item.</param>
    public static string? CreatorName(this IPublishedContent content) =>
        content.CreatorName(UserService);

    /// <summary>
    ///     Gets the name of the content item writer.
    /// </summary>
    /// <param name="content">The content item.</param>
    public static string? WriterName(this IPublishedContent content) =>
        content.WriterName(UserService);

    /// <summary>
    ///     Gets the culture assigned to a document by domains, in the context of a current Uri.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <param name="current">An optional current Uri.</param>
    /// <returns>The culture assigned to the document by domains.</returns>
    /// <remarks>
    ///     <para>
    ///         In 1:1 multilingual setup, a document contains several cultures (there is not
    ///         one document per culture), and domains, withing the context of a current Uri, assign
    ///         a culture to that document.
    ///     </para>
    /// </remarks>
    public static string? GetCultureFromDomains(
        this IPublishedContent content,
        Uri? current = null)
        => content.GetCultureFromDomains(UmbracoContextAccessor, SiteDomainHelper, DomainCache, PublishedContentCache, DocumentNavigationQueryService, current);

    public static IEnumerable<PublishedSearchResult> SearchDescendants(
        this IPublishedContent content,
        string term,
        string? indexName = null)
        => content.SearchDescendants(ExamineManager, UmbracoContextAccessor, term, indexName);

    public static IEnumerable<PublishedSearchResult> SearchChildren(
        this IPublishedContent content,
        string term,
        string? indexName = null)
        => content.SearchChildren(ExamineManager, UmbracoContextAccessor, term, indexName);

}
