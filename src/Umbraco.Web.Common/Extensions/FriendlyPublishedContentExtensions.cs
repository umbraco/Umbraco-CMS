using System.Data;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Extensions;

public static class FriendlyPublishedContentExtensions
{
    private static IVariationContextAccessor VariationContextAccessor { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>();

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

    private static IPublishedSnapshot? PublishedSnapshot
    {
        get
        {
            if (!UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
            {
                return null;
            }

            return umbracoContext.PublishedSnapshot;
        }
    }

    private static IMediaTypeService MediaTypeService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IMediaTypeService>();

    private static IMemberTypeService MemberTypeService { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>();

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
        => parentNodes.DescendantsOrSelfOfType(VariationContextAccessor, docTypeAlias, culture);

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
        => parentNodes.DescendantsOrSelf<T>(VariationContextAccessor, culture);

    public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, string? culture = null)
        => content.Descendants(VariationContextAccessor, culture);

    public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, int level, string? culture = null)
        => content.Descendants(VariationContextAccessor, level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantsOfType(VariationContextAccessor, contentTypeAlias, culture);

    public static IEnumerable<T> Descendants<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(VariationContextAccessor, culture);

    public static IEnumerable<T> Descendants<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(VariationContextAccessor, level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        string? culture = null)
        => content.DescendantsOrSelf(VariationContextAccessor, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, int level, string? culture = null)
        => content.DescendantsOrSelf(VariationContextAccessor, level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantsOrSelfOfType(VariationContextAccessor, contentTypeAlias, culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(VariationContextAccessor, culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(VariationContextAccessor, level, culture);

    public static IPublishedContent? Descendant(this IPublishedContent content, string? culture = null)
        => content.Descendant(VariationContextAccessor, culture);

    public static IPublishedContent? Descendant(this IPublishedContent content, int level, string? culture = null)
        => content.Descendant(VariationContextAccessor, level, culture);

    public static IPublishedContent? DescendantOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantOfType(VariationContextAccessor, contentTypeAlias, culture);

    public static T? Descendant<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(VariationContextAccessor, culture);

    public static T? Descendant<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(VariationContextAccessor, level, culture);

    public static IPublishedContent DescendantOrSelf(this IPublishedContent content, string? culture = null)
        => content.DescendantOrSelf(VariationContextAccessor, culture);

    public static IPublishedContent? DescendantOrSelf(this IPublishedContent content, int level, string? culture = null)
        => content.DescendantOrSelf(VariationContextAccessor, level, culture);

    public static IPublishedContent? DescendantOrSelfOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.DescendantOrSelfOfType(VariationContextAccessor, contentTypeAlias, culture);

    public static T? DescendantOrSelf<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(VariationContextAccessor, culture);

    public static T? DescendantOrSelf<T>(this IPublishedContent content, int level, string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(VariationContextAccessor, level, culture);

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
    public static IEnumerable<IPublishedContent>? Children(this IPublishedContent content, string? culture = null)
        => content.Children(VariationContextAccessor, culture);

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
        => content.Children(VariationContextAccessor, predicate, culture);

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
        => content.ChildrenOfType(VariationContextAccessor, contentTypeAlias, culture);

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
        => content.Children<T>(VariationContextAccessor, culture);

    public static IPublishedContent? FirstChild(this IPublishedContent content, string? culture = null)
        => content.FirstChild(VariationContextAccessor, culture);

    /// <summary>
    ///     Gets the first child of the content, of a given content type.
    /// </summary>
    public static IPublishedContent? FirstChildOfType(this IPublishedContent content, string contentTypeAlias, string? culture = null)
        => content.FirstChildOfType(VariationContextAccessor, contentTypeAlias, culture);

    public static IPublishedContent? FirstChild(this IPublishedContent content, Func<IPublishedContent, bool> predicate, string? culture = null)
        => content.FirstChild(VariationContextAccessor, predicate, culture);

    public static IPublishedContent? FirstChild(this IPublishedContent content, Guid uniqueId, string? culture = null)
        => content.FirstChild(VariationContextAccessor, uniqueId, culture);

    public static T? FirstChild<T>(this IPublishedContent content, string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(VariationContextAccessor, culture);

    public static T? FirstChild<T>(this IPublishedContent content, Func<T, bool> predicate, string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild(VariationContextAccessor, predicate, culture);

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
        => content.Siblings(PublishedSnapshot, VariationContextAccessor, culture);

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
        => content.SiblingsOfType(PublishedSnapshot, VariationContextAccessor, contentTypeAlias, culture);

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
        => content.Siblings<T>(PublishedSnapshot, VariationContextAccessor, culture);

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
        => content.SiblingsAndSelf(PublishedSnapshot, VariationContextAccessor, culture);

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
        => content.SiblingsAndSelfOfType(PublishedSnapshot, VariationContextAccessor, contentTypeAlias, culture);

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
        => content.SiblingsAndSelf<T>(PublishedSnapshot, VariationContextAccessor, culture);

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
        => content.GetCultureFromDomains(UmbracoContextAccessor, SiteDomainHelper, current);

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
