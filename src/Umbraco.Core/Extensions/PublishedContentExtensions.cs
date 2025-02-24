// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Extensions;

public static class PublishedContentExtensions
{
    #region Name

    /// <summary>
    ///     Gets the name of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static string Name(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        // invariant has invariant value (whatever the requested culture)
        if (!content.ContentType.VariesByCulture())
        {
            return content.Cultures.TryGetValue(string.Empty, out PublishedCultureInfo? invariantInfos)
                ? invariantInfos.Name
                : string.Empty;
        }

        // handle context culture for variant
        if (culture == null)
        {
            culture = variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // get
        return culture != string.Empty && content.Cultures.TryGetValue(culture, out PublishedCultureInfo? infos)
            ? infos.Name
            : string.Empty;
    }

    #endregion

    #region Url segment

    /// <summary>
    ///     Gets the URL segment of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture">
    ///     The specific culture to get the URL segment for. If null is used the current culture is used
    ///     (Default is null).
    /// </param>
    [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in V16.")]
    public static string? UrlSegment(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        // invariant has invariant value (whatever the requested culture)
        if (!content.ContentType.VariesByCulture())
        {
            return content.Cultures.TryGetValue(string.Empty, out PublishedCultureInfo? invariantInfos)
                ? invariantInfos.UrlSegment
                : null;
        }

        // handle context culture for variant
        if (culture == null)
        {
            culture = variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // get
        return culture != string.Empty && content.Cultures.TryGetValue(culture, out PublishedCultureInfo? infos)
            ? infos.UrlSegment
            : null;
    }

    #endregion

    #region IsComposedOf

    /// <summary>
    ///     Gets a value indicating whether the content is of a content type composed of the given alias
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="alias">The content type alias.</param>
    /// <returns>
    ///     A value indicating whether the content is of a content type composed of a content type identified by the
    ///     alias.
    /// </returns>
    public static bool IsComposedOf(this IPublishedContent content, string alias) =>
        content.ContentType.CompositionAliases.InvariantContains(alias);

    #endregion

    #region Axes: parent

    // Parent is native

    /// <summary>
    ///     Gets the parent of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The parent of content, of the given content type, else null.</returns>
    public static T? Parent<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.GetParent(publishedCache, navigationQueryService) as T;
    }

    private static IPublishedContent? GetParent(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
    {
        IPublishedContent? parent;
        if (navigationQueryService.TryGetParentKey(content.Key, out Guid? parentKey))
        {
            parent = parentKey.HasValue ? publishedCache.GetById(parentKey.Value) : null;
        }
        else
        {
            throw new KeyNotFoundException($"Content with key '{content.Key}' was not found in the in-memory navigation structure.");
        }

        return parent;
    }

    #endregion

    #region Url

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
    public static string Url(this IPublishedContent content, IPublishedUrlProvider publishedUrlProvider, string? culture = null, UrlMode mode = UrlMode.Default)
    {
        if (publishedUrlProvider == null)
        {
            throw new InvalidOperationException(
                "Cannot resolve a Url when Current.UmbracoContext.UrlProvider is null.");
        }

        switch (content.ContentType.ItemType)
        {
            case PublishedItemType.Content:
                return publishedUrlProvider.GetUrl(content, mode, culture);

            case PublishedItemType.Media:
                return publishedUrlProvider.GetMediaUrl(content, mode, culture);

            default:
                throw new NotSupportedException();
        }
    }

    #endregion

    #region Culture

    /// <summary>
    ///     Determines whether the content has a culture.
    /// </summary>
    /// <remarks>Culture is case-insensitive.</remarks>
    public static bool HasCulture(this IPublishedContent content, string? culture)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.Cultures.ContainsKey(culture ?? string.Empty);
    }

    /// <summary>
    ///     Determines whether the content is invariant, or has a culture.
    /// </summary>
    /// <remarks>Culture is case-insensitive.</remarks>
    public static bool IsInvariantOrHasCulture(this IPublishedContent content, string culture)
        => !content.ContentType.VariesByCulture() || content.Cultures.ContainsKey(culture ?? string.Empty);

    /// <summary>
    ///     Filters a sequence of <see cref="IPublishedContent" /> to return invariant items, and items that are published for
    ///     the specified culture.
    /// </summary>
    /// <param name="contents">The content items.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null).
    /// </param>
    internal static IEnumerable<T> WhereIsInvariantOrHasCulture<T>(this IEnumerable<T> contents, IVariationContextAccessor variationContextAccessor, string? culture = null)
        where T : class, IPublishedContent
    {
        if (contents == null)
        {
            throw new ArgumentNullException(nameof(contents));
        }

        culture = culture ?? variationContextAccessor.VariationContext?.Culture ?? string.Empty;

        // either does not vary by culture, or has the specified culture
        return contents.Where(x => !x.ContentType.VariesByCulture() || HasCulture(x, culture));
    }

    /// <summary>
    ///     Gets the culture date of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static DateTime CultureDate(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string? culture = null)
    {
        // invariant has invariant value (whatever the requested culture)
        if (!content.ContentType.VariesByCulture())
        {
            return content.UpdateDate;
        }

        // handle context culture for variant
        if (culture == null)
        {
            culture = variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // get
        return culture != string.Empty && content.Cultures.TryGetValue(culture, out PublishedCultureInfo? infos)
            ? infos.Date
            : DateTime.MinValue;
    }

    #endregion

    #region Template

    /// <summary>
    ///     Returns the current template Alias
    /// </summary>
    /// <returns>Empty string if none is set.</returns>
    public static string GetTemplateAlias(this IPublishedContent content, IFileService fileService)
    {
        if (content.TemplateId.HasValue == false)
        {
            return string.Empty;
        }

        ITemplate? template = fileService.GetTemplate(content.TemplateId.Value);
        return template?.Alias ?? string.Empty;
    }

    public static bool IsAllowedTemplate(this IPublishedContent content, IContentTypeService contentTypeService, WebRoutingSettings webRoutingSettings, int templateId) =>
        content.IsAllowedTemplate(contentTypeService, webRoutingSettings.DisableAlternativeTemplates, webRoutingSettings.ValidateAlternativeTemplates, templateId);

    public static bool IsAllowedTemplate(this IPublishedContent content, IContentTypeService contentTypeService, bool disableAlternativeTemplates, bool validateAlternativeTemplates, int templateId)
    {
        if (disableAlternativeTemplates)
        {
            return content.TemplateId == templateId;
        }

        if (content.TemplateId == templateId || !validateAlternativeTemplates)
        {
            return true;
        }

        IContentType? publishedContentContentType = contentTypeService.Get(content.ContentType.Id);
        if (publishedContentContentType == null)
        {
            throw new NullReferenceException("No content type returned for published content (contentType='" +
                                             content.ContentType.Id + "')");
        }

        return publishedContentContentType.IsAllowedTemplate(templateId);
    }

    public static bool IsAllowedTemplate(this IPublishedContent content, IFileService fileService, IContentTypeService contentTypeService, bool disableAlternativeTemplates, bool validateAlternativeTemplates, string templateAlias)
    {
        ITemplate? template = fileService.GetTemplate(templateAlias);
        return template != null && content.IsAllowedTemplate(contentTypeService, disableAlternativeTemplates, validateAlternativeTemplates, template.Id);
    }

    #endregion

    #region HasValue, Value, Value<T>

    /// <summary>
    ///     Gets a value indicating whether the content has a value for a property identified by its alias.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback">The published value fallback implementation.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <returns>A value indicating whether the content has a value for the property identified by the alias.</returns>
    /// <remarks>Returns true if HasValue is true, or a fallback strategy can provide a value.</remarks>
    public static bool HasValue(this IPublishedContent content, IPublishedValueFallback publishedValueFallback, string alias, string? culture = null, string? segment = null, Fallback fallback = default)
    {
        IPublishedProperty? property = content.GetProperty(alias);

        // if we have a property, and it has a value, return that value
        if (property != null && property.HasValue(culture, segment))
        {
            return true;
        }

        // else let fallback try to get a value
        return publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, null, out _, out _);
    }

    /// <summary>
    ///     Gets the value of a content's property identified by its alias, if it exists, otherwise a default value.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback">The published value fallback implementation.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
    public static object? Value(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        object? defaultValue = default)
    {
        IPublishedProperty? property = content.GetProperty(alias);

        // if we have a property, and it has a value, return that value
        if (property != null && property.HasValue(culture, segment))
        {
            return property.GetValue(culture, segment);
        }

        // else let fallback try to get a value
        if (publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out var value, out property))
        {
            return value;
        }

        // else... if we have a property, at least let the converter return its own
        // vision of 'no value' (could be an empty enumerable)
        return property?.GetValue(culture, segment);
    }

    /// <summary>
    ///     Gets the value of a content's property identified by its alias, converted to a specified type.
    /// </summary>
    /// <typeparam name="T">The target property type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback">The published value fallback implementation.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
    public static T? Value<T>(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        T? defaultValue = default)
    {
        IPublishedProperty? property = content.GetProperty(alias);

        // if we have a property, and it has a value, return that value
        if (property != null && property.HasValue(culture, segment))
        {
            return property.Value<T>(publishedValueFallback, culture, segment);
        }

        // else let fallback try to get a value
        if (publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out T? value, out property))
        {
            return value;
        }

        // else... if we have a property, at least let the converter return its own
        // vision of 'no value' (could be an empty enumerable) - otherwise, default
        return property == null ? default : property.Value<T>(publishedValueFallback, culture, segment);
    }

    #endregion

    #region IsSomething: misc.

    /// <summary>
    ///     Determines whether the specified content is a specified content type.
    /// </summary>
    /// <param name="content">The content to determine content type of.</param>
    /// <param name="docTypeAlias">The alias of the content type to test against.</param>
    /// <returns>True if the content is of the specified content type; otherwise false.</returns>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias) =>
        content.ContentType.Alias.InvariantEquals(docTypeAlias);

    /// <summary>
    ///     Determines whether the specified content is a specified content type or it's derived types.
    /// </summary>
    /// <param name="content">The content to determine content type of.</param>
    /// <param name="docTypeAlias">The alias of the content type to test against.</param>
    /// <param name="recursive">
    ///     When true, recurses up the content type tree to check inheritance; when false just calls
    ///     IsDocumentType(this IPublishedContent content, string docTypeAlias).
    /// </param>
    /// <returns>True if the content is of the specified content type or a derived content type; otherwise false.</returns>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias, bool recursive)
    {
        if (content.IsDocumentType(docTypeAlias))
        {
            return true;
        }

        return recursive && content.IsComposedOf(docTypeAlias);
    }

    #endregion

    #region IsSomething: equality

    public static bool IsEqual(this IPublishedContent content, IPublishedContent other) => content.Id == other.Id;

    public static bool IsNotEqual(this IPublishedContent content, IPublishedContent other) =>
        content.IsEqual(other) == false;

    #endregion

    #region IsSomething: ancestors and descendants

    public static bool IsDescendant(this IPublishedContent content, IPublishedContent other) =>
        other.Level < content.Level && content.Path.InvariantStartsWith(other.Path.EnsureEndsWith(','));

    public static bool IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other) =>
        content.Path.InvariantEquals(other.Path) || content.IsDescendant(other);

    public static bool IsAncestor(this IPublishedContent content, IPublishedContent other) =>
        content.Level < other.Level && other.Path.InvariantStartsWith(content.Path.EnsureEndsWith(','));

    public static bool IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other) =>
        other.Path.InvariantEquals(content.Path) || content.IsAncestor(other);

    #endregion

    #region Axes: ancestors, ancestors-or-self

    // as per XPath 1.0 specs �2.2,
    // - the ancestor axis contains the ancestors of the context node; the ancestors of the context node consist
    //   of the parent of context node and the parent's parent and so on; thus, the ancestor axis will always
    //   include the root node, unless the context node is the root node.
    // - the ancestor-or-self axis contains the context node and the ancestors of the context node; thus,
    //   the ancestor axis will always include the root node.
    //
    // as per XPath 2.0 specs �3.2.1.1,
    // - the ancestor axis is defined as the transitive closure of the parent axis; it contains the ancestors
    //   of the context node (the parent, the parent of the parent, and so on) - The ancestor axis includes the
    //   root node of the tree in which the context node is found, unless the context node is the root node.
    // - the ancestor-or-self axis contains the context node and the ancestors of the context node; thus,
    //   the ancestor-or-self axis will always include the root node.
    //
    // the ancestor and ancestor-or-self axis are reverse axes ie they contain the context node or nodes that
    // are before the context node in document order.
    //
    // document order is defined by �2.4.1 as:
    // - the root node is the first node.
    // - every node occurs before all of its children and descendants.
    // - the relative order of siblings is the order in which they occur in the children property of their parent node.
    // - children and descendants occur before following siblings.

    /// <summary>
    ///     Gets the ancestors of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>The ancestors of the content, in down-top order.</returns>
    /// <remarks>Does not consider the content itself.</remarks>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService) =>
        content.AncestorsOrSelf(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            false,
            null);

    /// <summary>
    ///     Gets the ancestors of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The ancestors of the content, in down-top order.</returns>
    /// <remarks>Does not consider the content itself.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService) => Ancestors(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    /// <summary>
    ///     Gets the ancestors of the content, at a level lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The ancestors of the content, at a level lesser or equal to the specified level, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Only content that are "high enough" in the tree are returned.</remarks>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel) =>
        content.AncestorsOrSelf(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            false,
            n => n.Level <= maxLevel);

    /// <summary>
    ///     Gets the ancestors of the content, at a level lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The ancestors of the content, at a level lesser or equal to the specified level, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Only content that are "high enough" in the tree are returned.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel) =>
        Ancestors(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);


    /// <summary>
    ///     Gets the ancestors of the content, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.EnumerateAncestorsOrSelfInternal(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            false,
            contentTypeAlias);
    }

    /// <summary>
    ///     Gets the ancestors of the content, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias) =>
        Ancestors(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias);

    /// <summary>
    ///     Gets the ancestors of the content, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent =>
        content.Ancestors(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService).OfType<T>();

    /// <summary>
    ///     Gets the ancestors of the content, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent =>
        Ancestors<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    /// <summary>
    ///     Gets the ancestors of the content, at a level lesser or equal to a specified level, and of a specified content
    ///     type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     The ancestors of the content, at a level lesser or equal to the specified level, and of the specified
    ///     content type, in down-top order.
    /// </returns>
    /// <remarks>
    ///     Does not consider the content itself. Only content that are "high enough" in the trees, and of the
    ///     specified content type, are returned.
    /// </remarks>
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent =>
        content.Ancestors(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, maxLevel).OfType<T>();

    /// <summary>
    ///     Gets the ancestors of the content, at a level lesser or equal to a specified level, and of a specified content
    ///     type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     The ancestors of the content, at a level lesser or equal to the specified level, and of the specified
    ///     content type, in down-top order.
    /// </returns>
    /// <remarks>
    ///     Does not consider the content itself. Only content that are "high enough" in the trees, and of the
    ///     specified content type, are returned.
    /// </remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent =>
        Ancestors<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);

    /// <summary>
    ///     Gets the content and its ancestors.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>The content and its ancestors, in down-top order.</returns>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService) =>
        content.AncestorsOrSelf(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            true,
            null);

    /// <summary>
    ///     Gets the content and its ancestors.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The content and its ancestors, in down-top order.</returns>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService) =>
        AncestorsOrSelf(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    /// <summary>
    ///     Gets the content and its ancestors, at a level lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     The content and its ancestors, at a level lesser or equal to the specified level,
    ///     in down-top order.
    /// </returns>
    /// <remarks>
    ///     Only content that are "high enough" in the tree are returned. So it may or may not begin
    ///     with the content itself, depending on its level.
    /// </remarks>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel) =>
        content.AncestorsOrSelf(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            true,
            n => n.Level <= maxLevel);

    /// <summary>
    ///     Gets the content and its ancestors, at a level lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     The content and its ancestors, at a level lesser or equal to the specified level,
    ///     in down-top order.
    /// </returns>
    /// <remarks>
    ///     Only content that are "high enough" in the tree are returned. So it may or may not begin
    ///     with the content itself, depending on its level.
    /// </remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel) =>
        AncestorsOrSelf(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);

    /// <summary>
    ///     Gets the content and its ancestors, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
    /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.EnumerateAncestorsOrSelfInternal(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            true,
            contentTypeAlias);
    }

    /// <summary>
    ///     Gets the content and its ancestors, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
    /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias) =>
        AncestorsOrSelf(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias);

    /// <summary>
    ///     Gets the content and its ancestors, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
    /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent =>
        content.AncestorsOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService).OfType<T>();

    /// <summary>
    ///     Gets the content and its ancestors, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
    /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent => AncestorsOrSelf<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    /// <summary>
    ///     Gets the content and its ancestor, at a lever lesser or equal to a specified level, and of a specified content
    ///     type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     The content and its ancestors, at a level lesser or equal to the specified level, and of the specified
    ///     content type, in down-top order.
    /// </returns>
    /// <remarks>May or may not begin with the content itself, depending on its level and content type.</remarks>
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent =>
        content.AncestorsOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, maxLevel).OfType<T>();

    /// <summary>
    ///     Gets the content and its ancestor, at a lever lesser or equal to a specified level, and of a specified content
    ///     type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     The content and its ancestors, at a level lesser or equal to the specified level, and of the specified
    ///     content type, in down-top order.
    /// </returns>
    /// <remarks>May or may not begin with the content itself, depending on its level and content type.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent => AncestorsOrSelf<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);

    /// <summary>
    ///     Gets the ancestor of the content, ie its parent.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The ancestor of the content.</returns>
    /// <remarks>This method is here for consistency purposes but does not make much sense.</remarks>
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        => content.GetParent(publishedCache, navigationQueryService);

    /// <summary>
    ///     Gets the nearest ancestor of the content, at a lever lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, at a level lesser or equal to the specified level.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel) =>
        content.EnumerateAncestors(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            false).FirstOrDefault(x => x.Level <= maxLevel);

    /// <summary>
    ///     Gets the nearest ancestor of the content, at a lever lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, at a level lesser or equal to the specified level.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel) =>
        Ancestor(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);

    /// <summary>
    ///     Gets the nearest ancestor of the content, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content
            .EnumerateAncestorsOrSelfInternal(
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                publishStatusQueryService,
                false,
                contentTypeAlias).FirstOrDefault();
    }

    /// <summary>
    ///     Gets the nearest ancestor of the content, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias) =>
        Ancestor(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias);

    /// <summary>
    ///     Gets the nearest ancestor of the content, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent =>
        content.Ancestors<T>(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService).FirstOrDefault();

    /// <summary>
    ///     Gets the nearest ancestor of the content, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent =>
        Ancestor<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    /// <summary>
    ///     Gets the nearest ancestor of the content, at the specified level and of the specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The ancestor of the content, at the specified level and of the specified content type.</returns>
    /// <remarks>
    ///     Does not consider the content itself. If the ancestor at the specified level is
    ///     not of the specified type, returns <c>null</c>.
    /// </remarks>
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent =>
        content.Ancestors<T>(
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                publishStatusQueryService,
                maxLevel).FirstOrDefault();

    /// <summary>
    ///     Gets the nearest ancestor of the content, at the specified level and of the specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The ancestor of the content, at the specified level and of the specified content type.</returns>
    /// <remarks>
    ///     Does not consider the content itself. If the ancestor at the specified level is
    ///     not of the specified type, returns <c>null</c>.
    /// </remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent =>
        Ancestor<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);

    /// <summary>
    ///     Gets the content or its nearest ancestor.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The content.</returns>
    /// <remarks>This method is here for consistency purposes but does not make much sense.</remarks>
    public static IPublishedContent AncestorOrSelf(this IPublishedContent content) => content;

    /// <summary>
    ///     Gets the content or its nearest ancestor, at a lever lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, at a level lesser or equal to the specified level.</returns>
    /// <remarks>May or may not return the content itself depending on its level. May return <c>null</c>.</remarks>
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel) =>
        content.EnumerateAncestors(
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                publishStatusQueryService,
                true)
            .FirstOrDefault(x => x.Level <= maxLevel) ?? content;

    /// <summary>
    ///     Gets the content or its nearest ancestor, at a lever lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, at a level lesser or equal to the specified level.</returns>
    /// <remarks>May or may not return the content itself depending on its level. May return <c>null</c>.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel) =>
        AncestorOrSelf(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);

    /// <summary>
    ///     Gets the content or its nearest ancestor, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
    /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content
            .EnumerateAncestorsOrSelfInternal(
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                publishStatusQueryService,
                true,
                contentTypeAlias)
            .FirstOrDefault() ?? content;
    }

    /// <summary>
    ///     Gets the content or its nearest ancestor, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
    /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias) => AncestorOrSelf(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias);

    /// <summary>
    ///     Gets the content or its nearest ancestor, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
    /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent =>
        content.AncestorsOrSelf<T>(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService).FirstOrDefault();

    /// <summary>
    ///     Gets the content or its nearest ancestor, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
    /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent =>
        AncestorOrSelf<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    /// <summary>
    ///     Gets the content or its nearest ancestor, at a lever lesser or equal to a specified level, and of a specified
    ///     content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns></returns>
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent =>
        content.AncestorsOrSelf<T>(
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                publishStatusQueryService,
                maxLevel)
            .FirstOrDefault();

    /// <summary>
    ///     Gets the content or its nearest ancestor, at a lever lesser or equal to a specified level, and of a specified
    ///     content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="maxLevel">The level.</param>
    /// <returns></returns>
    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent =>
        AncestorOrSelf<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            maxLevel);

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        bool orSelf,
        Func<IPublishedContent, bool>? func)
    {
        IEnumerable<IPublishedContent> ancestorsOrSelf = content.EnumerateAncestors(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            orSelf);
        return func == null ? ancestorsOrSelf : ancestorsOrSelf.Where(func);
    }

    [Obsolete("Use the overload with IVariationContextAccessor and IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        bool orSelf,
        Func<IPublishedContent, bool>? func) =>
        AncestorsOrSelf(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            orSelf,
            func);

    /// <summary>
    ///     Enumerates ancestors of the content, bottom-up.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="orSelf">Indicates whether the content should be included.</param>
    /// <returns>Enumerates bottom-up ie walking up the tree (parent, grand-parent, etc).</returns>
    internal static IEnumerable<IPublishedContent> EnumerateAncestors(
        this IPublishedContent? content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        bool orSelf)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.EnumerateAncestorsOrSelfInternal(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, orSelf);
    }

    #endregion

    #region Axes: breadcrumbs

    /// <summary>
    ///     Gets the breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="andSelf">Indicates whether the specified content should be included.</param>
    /// <returns>
    ///     The breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" />.
    /// </returns>
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        bool andSelf = true) =>
        content.AncestorsOrSelf(publishedCache, navigationQueryService, andSelf, null).Reverse();

    /// <summary>
    ///     Gets the breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level
    ///     higher or equal to <paramref name="minLevel" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="minLevel">The minimum level.</param>
    /// <param name="andSelf">Indicates whether the specified content should be included.</param>
    /// <returns>
    ///     The breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level higher
    ///     or equal to <paramref name="minLevel" />.
    /// </returns>
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int minLevel,
        bool andSelf = true) =>
        content.AncestorsOrSelf(publishedCache, navigationQueryService, andSelf, n => n.Level >= minLevel).Reverse();

    /// <summary>
    ///     Gets the breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level
    ///     higher or equal to the specified root content type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The root content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="andSelf">Indicates whether the specified content should be included.</param>
    /// <returns>
    ///     The breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level higher
    ///     or equal to the specified root content type <typeparamref name="T" />.
    /// </returns>
    public static IEnumerable<IPublishedContent> Breadcrumbs<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        bool andSelf = true)
        where T : class, IPublishedContent
    {
        static IEnumerable<IPublishedContent> TakeUntil(IEnumerable<IPublishedContent> source, Func<IPublishedContent, bool> predicate)
        {
            foreach (IPublishedContent item in source)
            {
                yield return item;
                if (predicate(item))
                {
                    yield break;
                }
            }
        }

        return TakeUntil(content.AncestorsOrSelf(publishedCache, navigationQueryService, andSelf, null), n => n is T).Reverse();
    }

    #endregion

    #region Axes: descendants, descendants-or-self

    /// <summary>
    ///     Returns all DescendantsOrSelf of all content referenced
    /// </summary>
    /// <param name="parentNodes"></param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="docTypeAlias"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
    /// </remarks>
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string docTypeAlias,
        string? culture = null) => parentNodes.SelectMany(x =>
        x.DescendantsOrSelfOfType(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, docTypeAlias, culture));

    /// <summary>
    ///     Returns all DescendantsOrSelf of all content referenced
    /// </summary>
    /// <param name="parentNodes"></param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="docTypeAlias"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
    /// </remarks>
    [Obsolete("Use the overload with IPublishStatusQueryService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string docTypeAlias,
        string? culture = null) =>
        DescendantsOrSelfOfType(
            parentNodes,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            docTypeAlias,
            culture);

    /// <summary>
    ///     Returns all DescendantsOrSelf of all content referenced
    /// </summary>
    /// <param name="parentNodes"></param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
    /// </remarks>
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        parentNodes.SelectMany(x => x.DescendantsOrSelf<T>(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture));

    /// <summary>
    ///     Returns all DescendantsOrSelf of all content referenced
    /// </summary>
    /// <param name="parentNodes"></param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
    /// </remarks>
    [Obsolete("Use the overload with IPublishStatusQueryService instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantsOrSelf<T>(
            parentNodes,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    // as per XPath 1.0 specs �2.2,
    // - the descendant axis contains the descendants of the context node; a descendant is a child or a child of a child and so on; thus
    //   the descendant axis never contains attribute or namespace nodes.
    // - the descendant-or-self axis contains the context node and the descendants of the context node.
    //
    // as per XPath 2.0 specs �3.2.1.1,
    // - the descendant axis is defined as the transitive closure of the child axis; it contains the descendants of the context node (the
    //   children, the children of the children, and so on).
    // - the descendant-or-self axis contains the context node and the descendants of the context node.
    //
    // the descendant and descendant-or-self axis are forward axes ie they contain the context node or nodes that are after the context
    // node in document order.
    //
    // document order is defined by �2.4.1 as:
    // - the root node is the first node.
    // - every node occurs before all of its children and descendants.
    // - the relative order of siblings is the order in which they occur in the children property of their parent node.
    // - children and descendants occur before following siblings.
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.DescendantsOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, false, null, culture);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null) =>
        Descendants(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) =>
        content.DescendantsOrSelf(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            false,
            p => p.Level >= level,
            culture);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null) =>
        Descendants(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        content.EnumerateDescendantsOrSelfInternal(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            culture,
            false,
            contentTypeAlias);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantsOfType(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);

    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Descendants(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Descendants(variationContextAccessor, publishedCache, navigationQueryService, culture).OfType<T>();

    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Descendants(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, level, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        Descendants<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.DescendantsOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, true, null, culture);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null) =>
       DescendantsOrSelf(
           content,
           variationContextAccessor,
           publishedCache,
           navigationQueryService,
           StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
           culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) =>
        content.DescendantsOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, true, p => p.Level >= level, culture);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null) =>
        DescendantsOrSelf(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        content.EnumerateDescendantsOrSelfInternal(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            culture,
            true,
            contentTypeAlias);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantsOrSelfOfType(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.DescendantsOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.DescendantsOrSelf<T>(
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
                culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.DescendantsOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, level, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantsOrSelf<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.Children(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null) =>
        Descendant(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) => content
        .EnumerateDescendants(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, false, culture).FirstOrDefault(x => x.Level == level);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null) =>
        content
        .EnumerateDescendants(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            false,
            culture)
        .FirstOrDefault(x => x.Level == level);

    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) => content
            .EnumerateDescendantsOrSelfInternal(
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                publishStatusQueryService,
                culture,
                false,
                contentTypeAlias)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantOfType(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);

    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.EnumerateDescendants(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, false, culture).FirstOrDefault(x => x is T) as T;

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        Descendant<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Descendant(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, level, culture) as T;

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        Descendant<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    public static IPublishedContent DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.EnumerateDescendants(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                true,
                culture)
            .FirstOrDefault() ??
        content;

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IPublishedContent DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        DescendantOrSelf(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) => content
        .EnumerateDescendants(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            true,
            culture)
        .FirstOrDefault(x => x.Level == level);

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null) =>
        DescendantOrSelf(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) => content
        .EnumerateDescendantsOrSelfInternal(
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            publishStatusQueryService,
            culture,
            true,
            contentTypeAlias)
        .FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantOrSelfOfType(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);

    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.EnumerateDescendants(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, true, culture).FirstOrDefault(x => x is T) as T;

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantOrSelf<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.DescendantOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, level, culture) as T;

    [Obsolete("Use the overload with IPublishStatusQuery instead, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantOrSelf<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    internal static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        bool orSelf,
        Func<IPublishedContent, bool>? func,
        string? culture = null) =>
        content.EnumerateDescendants(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, orSelf, culture)
        .Where(x => func == null || func(x));

    internal static IEnumerable<IPublishedContent> EnumerateDescendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        bool orSelf,
        string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        foreach (IPublishedContent desc in content.EnumerateDescendantsOrSelfInternal(
                     variationContextAccessor,
                     publishedCache,
                     navigationQueryService,
                     publishStatusQueryService,
                     culture,
                     orSelf))
        {
            yield return desc;
        }
    }

    internal static IEnumerable<IPublishedContent> EnumerateDescendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
    {
        yield return content;

        foreach (IPublishedContent desc in content.EnumerateDescendantsOrSelfInternal(
                     variationContextAccessor,
                     publishedCache,
                     navigationQueryService,
                     publishStatusQueryService,
                     culture,
                     false))
        {
            yield return desc;
        }
    }

    #endregion

    #region Axes: children

    /// <summary>
    ///     Gets the children of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to get the URL children for. Default is null which will use the current culture in
    ///     <see cref="VariationContext" />
    /// </param>
    /// <param name="publishedCache"></param>
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
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
    {
        IEnumerable<IPublishedContent> children = GetChildren(navigationQueryService, publishedCache, content.Key, publishStatusQueryService, variationContextAccessor, null, culture);

        return children.FilterByCulture(culture, variationContextAccessor);
    }

    /// <summary>
    ///     Gets the children of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to get the URL children for. Default is null which will use the current culture in
    ///     <see cref="VariationContext" />
    /// </param>
    /// <param name="publishedCache"></param>
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
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
    {
        IPublishStatusQueryService publishStatusQueryService = StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>();
        return Children(content, variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture);
    }

    /// <summary>
    ///     Gets the children of the content, filtered by a predicate.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"> The accessor for VariationContext</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The children of the content, filtered by the predicate.</returns>
    /// <remarks>
    ///     <para>Children are sorted by their sortOrder.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null) =>
        content.Children(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture).Where(predicate);

    /// <summary>
    ///     Gets the children of the content, filtered by a predicate.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"> The accessor for VariationContext</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The children of the content, filtered by the predicate.</returns>
    /// <remarks>
    ///     <para>Children are sorted by their sortOrder.</para>
    /// </remarks>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null) =>
        content.Children(variationContextAccessor, publishedCache, navigationQueryService, culture).Where(predicate);

    /// <summary>
    ///     Gets the children of the content, of any of the specified types.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="variationContextAccessor">The accessor for the VariationContext</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The children of the content, of any of the specified types.</returns>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? contentTypeAlias,
        string? culture = null)
    {
        IPublishStatusQueryService publishStatusQueryService = StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>();
        return ChildrenOfType(content, variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, contentTypeAlias, culture);
    }

    /// <summary>
    ///     Gets the children of the content, of any of the specified types.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="variationContextAccessor">The accessor for the VariationContext</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The children of the content, of any of the specified types.</returns>
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? contentTypeAlias,
        string? culture = null)
    {
        IEnumerable<IPublishedContent> children = contentTypeAlias is not null
            ? GetChildren(navigationQueryService, publishedCache, content.Key, publishStatusQueryService, variationContextAccessor, contentTypeAlias, culture)
            : [];

        return children.FilterByCulture(culture, variationContextAccessor);
    }

    /// <summary>
    ///     Gets the children of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">The accessor for the VariationContext</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The children of content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Children are sorted by their sortOrder.</para>
    /// </remarks>
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Children(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture).OfType<T>();

    /// <summary>
    ///     Gets the children of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">The accessor for the VariationContext</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The children of content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Children are sorted by their sortOrder.</para>
    /// </remarks>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Children(variationContextAccessor, publishedCache, navigationQueryService, culture).OfType<T>();

    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.Children(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null) =>
        FirstChild(content, variationContextAccessor, publishedCache, navigationQueryService, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);

    /// <summary>
    ///     Gets the first child of the content, of a given content type.
    /// </summary>
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) => content
        .ChildrenOfType(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, contentTypeAlias, culture)
        .FirstOrDefault();

    /// <summary>
    ///     Gets the first child of the content, of a given content type.
    /// </summary>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        FirstChildOfType(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);

    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, predicate, culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null) =>
        FirstChild(
                content,
                variationContextAccessor,
                publishedCache,
                navigationQueryService,
                StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
                predicate,
                culture);

    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Guid uniqueId,
        string? culture = null) => content
        .Children(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, x => x.Key == uniqueId, culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Guid uniqueId,
        string? culture = null) =>
        FirstChild(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            uniqueId,
            culture);

    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Children<T>(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        FirstChild<T>(content, variationContextAccessor, publishedCache, navigationQueryService, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);

    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Children<T>(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture)?.FirstOrDefault(predicate);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent =>
        FirstChild<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            predicate,
            culture);

    #endregion

    #region Axes: siblings

    /// <summary>
    ///     Gets the siblings of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The navigation service</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache">The content cache instance.</param>
    /// <returns>The siblings of the content.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        SiblingsAndSelf(content, publishedCache, navigationQueryService, variationContextAccessor, publishStatusQueryService, culture)
            ?.Where(x => x.Id != content.Id) ?? Enumerable.Empty<IPublishedContent>();

    /// <summary>
    ///     Gets the siblings of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The navigation service</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache">The content cache instance.</param>
    /// <returns>The siblings of the content.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        Siblings(
            content,
            publishedCache,
            navigationQueryService,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The siblings of the content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent> SiblingsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        SiblingsAndSelfOfType(content, variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, contentTypeAlias, culture)
            .Where(x => x.Id != content.Id);

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The siblings of the content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        SiblingsOfType(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="publishedCache"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<T> Siblings<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        SiblingsAndSelf<T>(content, variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture)
            ?.Where(x => x.Id != content.Id) ?? Enumerable.Empty<T>();

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="publishedCache"></param>
    /// <param name="navigationQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> Siblings<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        Siblings<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">Cache instance.</param>
    /// <param name="navigationQueryService">The navigation service.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content including the node itself.</returns>
    public static IEnumerable<IPublishedContent>? SiblingsAndSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
    {
        var success = navigationQueryService.TryGetParentKey(content.Key, out Guid? parentKey);

        if (success is false || parentKey is null)
        {
            if (navigationQueryService.TryGetRootKeys(out IEnumerable<Guid> childrenKeys) is false)
            {
                return null;
            }

            culture ??= variationContextAccessor.VariationContext?.Culture ?? string.Empty;
            return childrenKeys
                .Where(x => publishStatusQueryService.IsDocumentPublished(x , culture))
                .Select(publishedCache.GetById)
                .WhereNotNull()
                .WhereIsInvariantOrHasCulture(variationContextAccessor, culture);
        }

        return navigationQueryService.TryGetChildrenKeys(parentKey.Value, out IEnumerable<Guid> siblingKeys) is false
            ? null
            : siblingKeys.Select(publishedCache.GetById).WhereNotNull();
    }

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">Cache instance.</param>
    /// <param name="navigationQueryService">The navigation service.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content including the node itself.</returns>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent>? SiblingsAndSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        SiblingsAndSelf(
            content,
            publishedCache,
            navigationQueryService,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="publishedCache"></param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
    {
        var parentExists = navigationQueryService.TryGetParentKey(content.Key, out Guid? parentKey);

        IPublishedContent? parent = parentKey is null
            ? null
            : publishedCache.GetById(parentKey.Value);

        if (parentExists && parent is not null)
        {
            return parent.ChildrenOfType(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, contentTypeAlias, culture);
        }

        if (navigationQueryService.TryGetRootKeysOfType(contentTypeAlias, out IEnumerable<Guid> rootKeysOfType) is false)
        {
            return [];
        }

        culture ??= variationContextAccessor.VariationContext?.Culture ?? string.Empty;
        return rootKeysOfType
            .Where(x => publishStatusQueryService.IsDocumentPublished(x, culture))
            .Select(publishedCache.GetById)
            .WhereNotNull()
            .WhereIsInvariantOrHasCulture(variationContextAccessor, culture);
    }

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="navigationQueryService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="publishedCache"></param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        SiblingsAndSelfOfType(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishStatusQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
    {
        var parentSuccess = navigationQueryService.TryGetParentKey(content.Key, out Guid? parentKey);
        IPublishedContent? parent = parentKey is null ? null : publishedCache.GetById(parentKey.Value);

        if (parentSuccess is false || parent is null)
        {
            var rootSuccess = navigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
            if (rootSuccess is false)
            {
                return [];
            }

            culture ??= variationContextAccessor.VariationContext?.Culture ?? string.Empty;
            return rootKeys
                .Where(x => publishStatusQueryService.IsDocumentPublished(x, culture))
                .Select(publishedCache.GetById)
                .WhereNotNull()
                .WhereIsInvariantOrHasCulture(variationContextAccessor, culture)
                .OfType<T>();
        }

        return parent.Children<T>(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, culture);
    }

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent => SiblingsAndSelf<T>(
            content,
            variationContextAccessor,
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    #endregion

    #region Axes: custom

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>
    ///     The root content (ancestor or self at level 1) for the specified <paramref name="content" />.
    /// </returns>
    /// <remarks>
    ///     This is the same as calling
    ///     <see cref="AncestorOrSelf(IPublishedContent, int)" /> with <c>maxLevel</c>
    ///     set to 1.
    /// </remarks>
    public static IPublishedContent Root(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService) => content.AncestorOrSelf(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, 1);

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>
    ///     The root content (ancestor or self at level 1) for the specified <paramref name="content" />.
    /// </returns>
    /// <remarks>
    ///     This is the same as calling
    ///     <see cref="AncestorOrSelf(IPublishedContent, int)" /> with <c>maxLevel</c>
    ///     set to 1.
    /// </remarks>
    [Obsolete("Use the overload with IVariationContextAccessor & IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent Root(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService) => Root(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" /> if it's of the
    ///     specified content type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishStatusQueryService"></param>
    /// <returns>
    ///     The root content (ancestor or self at level 1) for the specified <paramref name="content" /> of content type
    ///     <typeparamref name="T" />.
    /// </returns>
    /// <remarks>
    ///     This is the same as calling
    ///     <see cref="AncestorOrSelf{T}(IPublishedContent, int)" /> with
    ///     <c>maxLevel</c> set to 1.
    /// </remarks>
    public static T? Root<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent =>
        content.AncestorOrSelf<T>(variationContextAccessor, publishedCache, navigationQueryService, publishStatusQueryService, 1);

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" /> if it's of the
    ///     specified content type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedCache">The content cache.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <returns>
    ///     The root content (ancestor or self at level 1) for the specified <paramref name="content" /> of content type
    ///     <typeparamref name="T" />.
    /// </returns>
    /// <remarks>
    ///     This is the same as calling
    ///     <see cref="AncestorOrSelf{T}(IPublishedContent, int)" /> with
    ///     <c>maxLevel</c> set to 1.
    /// </remarks>
    [Obsolete("Use the overload with IVariationContextAccessor & PublishStatusQueryService, scheduled for removal in v17")]
    public static T? Root<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent =>
        Root<T>(
            content,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            publishedCache,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>());

    #endregion

    #region Writer and creator

    public static string GetCreatorName(this IPublishedContent content, IUserService userService)
    {
        IProfile? user = userService.GetProfileById(content.CreatorId);
        return user?.Name ?? string.Empty;
    }

    public static string GetWriterName(this IPublishedContent content, IUserService userService)
    {
        IProfile? user = userService.GetProfileById(content.WriterId);
        return user?.Name ?? string.Empty;
    }

    #endregion

    #region Axes: children

    /// <summary>
    ///     Gets the children of the content in a DataTable.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaTypeService">The media type service.</param>
    /// <param name="memberTypeService">The member type service.</param>
    /// <param name="publishedUrlProvider">The published url provider.</param>
    /// <param name="contentTypeAliasFilter">An optional content type alias.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The children of the content.</returns>
    [Obsolete("This method is no longer used in Umbraco. The method will be removed in Umbraco 17.")]
    public static DataTable ChildrenAsTable(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IPublishedUrlProvider publishedUrlProvider,
        string contentTypeAliasFilter = "",
        string? culture = null)
        => GenerateDataTable(content, variationContextAccessor, publishedCache, navigationQueryService, contentTypeService, mediaTypeService, memberTypeService, publishedUrlProvider, contentTypeAliasFilter, culture);

    /// <summary>
    ///     Gets the children of the content in a DataTable.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="variationContextAccessor">Variation context accessor.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaTypeService">The media type service.</param>
    /// <param name="memberTypeService">The member type service.</param>
    /// <param name="publishedUrlProvider">The published url provider.</param>
    /// <param name="contentTypeAliasFilter">An optional content type alias.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="publishedCache"></param>
    /// <returns>The children of the content.</returns>
    private static DataTable GenerateDataTable(
        IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IPublishedUrlProvider publishedUrlProvider,
        string contentTypeAliasFilter = "",
        string? culture = null)
    {
        IPublishedContent? firstNode = contentTypeAliasFilter.IsNullOrWhiteSpace()
            ? content.Children(variationContextAccessor, publishedCache, navigationQueryService, culture)?.Any() ?? false
                ? content.Children(variationContextAccessor, publishedCache, navigationQueryService, culture)?.ElementAt(0)
                : null
            : content.Children(variationContextAccessor, publishedCache, navigationQueryService, culture)
                ?.FirstOrDefault(x => x.ContentType.Alias.InvariantEquals(contentTypeAliasFilter));
        if (firstNode == null)
        {
            // No children found
            return new DataTable();
        }

        // use new utility class to create table so that we don't have to maintain code in many places, just one
        DataTable dt = DataTableExtensions.GenerateDataTable(

            // pass in the alias of the first child node since this is the node type we're rendering headers for
            firstNode.ContentType.Alias,

            // pass in the callback to extract the Dictionary<string, string> of all defined aliases to their names
            alias => GetPropertyAliasesAndNames(contentTypeService, mediaTypeService, memberTypeService, alias),
            () =>
            {
                // here we pass in a callback to populate the datatable, yup its a bit ugly but it's already legacy and we just want to maintain code in one place.
                // create all row data
                List<Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>>>
                    tableData = DataTableExtensions.CreateTableData();
                IOrderedEnumerable<IPublishedContent>? children =
                    content.Children(variationContextAccessor, publishedCache, navigationQueryService)?.OrderBy(x => x.SortOrder);
                if (children is not null)
                {
                    // loop through each child and create row data for it
                    foreach (IPublishedContent n in children)
                    {
                        if (contentTypeAliasFilter.IsNullOrWhiteSpace() == false)
                        {
                            if (n.ContentType.Alias.InvariantEquals(contentTypeAliasFilter) == false)
                            {
                                continue; // skip this one, it doesn't match the filter
                            }
                        }

                        var standardVals = new Dictionary<string, object?>
                        {
                            { "Id", n.Id },
                            { "NodeName", n.Name(variationContextAccessor) },
                            { "NodeTypeAlias", n.ContentType.Alias },
                            { "CreateDate", n.CreateDate },
                            { "UpdateDate", n.UpdateDate },
                            { "CreatorId", n.CreatorId },
                            { "WriterId", n.WriterId },
                            { "Url", n.Url(publishedUrlProvider) },
                        };

                        var userVals = new Dictionary<string, object?>();
                        IEnumerable<IPublishedProperty> properties =
                            n.Properties?.Where(p => p.GetSourceValue() is not null) ??
                            Array.Empty<IPublishedProperty>();
                        foreach (IPublishedProperty p in properties)
                        {
                            // probably want the "object value" of the property here...
                            userVals[p.Alias] = p.GetValue();
                        }

                        // Add the row data
                        DataTableExtensions.AddRowData(tableData, standardVals, userVals);
                    }
                }

                return tableData;
            });
        return dt;
    }

    #endregion

    #region PropertyAliasesAndNames

    private static Func<IContentTypeService, IMediaTypeService, IMemberTypeService, string, Dictionary<string, string>>? _getPropertyAliasesAndNames;

    /// <summary>
    ///     This is used only for unit tests to set the delegate to look up aliases/names dictionary of a content type
    /// </summary>
    internal static Func<IContentTypeService, IMediaTypeService, IMemberTypeService, string, Dictionary<string, string>>
        GetPropertyAliasesAndNames
    {
        get => _getPropertyAliasesAndNames ?? GetAliasesAndNames;
        set => _getPropertyAliasesAndNames = value;
    }

    private static Dictionary<string, string> GetAliasesAndNames(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService, string alias)
    {
        IContentTypeBase? type = contentTypeService.Get(alias)
                                 ?? mediaTypeService.Get(alias)
                                 ?? (IContentTypeBase?)memberTypeService.Get(alias);
        Dictionary<string, string> fields = GetAliasesAndNames(type);

        // ensure the standard fields are there
        var stdFields = new Dictionary<string, string>
        {
            { "Id", "Id" },
            { "NodeName", "NodeName" },
            { "NodeTypeAlias", "NodeTypeAlias" },
            { "CreateDate", "CreateDate" },
            { "UpdateDate", "UpdateDate" },
            { "CreatorId", "CreatorId" },
            { "WriterId", "WriterId" },
            { "Url", "Url" },
        };

        foreach (KeyValuePair<string, string> field in stdFields)
        {
            fields.TryAdd(field.Key, field.Value);
        }

        return fields;
    }

    private static Dictionary<string, string> GetAliasesAndNames(IContentTypeBase? contentType) =>
        contentType?.PropertyTypes.ToDictionary(x => x.Alias, x => x.Name) ?? new Dictionary<string, string>();



    #endregion


    public static IPublishedContent? Ancestor(this IPublishedContent content, int maxLevel)
    {
        return content.Ancestor(GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }


    public static IPublishedContent? Ancestor(this IPublishedContent content, string contentTypeAlias)
    {
        return content.Ancestor(GetPublishedCache(content), GetNavigationQueryService(content), contentTypeAlias);
    }


    public static T? Ancestor<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
    {
        return Ancestor<T>(content, GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }


    public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, int maxLevel)
    {
        return content.Ancestors(GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }


    public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, string contentTypeAlias)
    {
        return content.Ancestors(GetPublishedCache(content), GetNavigationQueryService(content), contentTypeAlias);
    }


    public static IEnumerable<T> Ancestors<T>(this IPublishedContent content)
        where T : class, IPublishedContent
    {
        return Ancestors<T>(content, GetPublishedCache(content), GetNavigationQueryService(content));
    }


    public static IEnumerable<T> Ancestors<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
    {
        return Ancestors<T>(content, GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }

    public static IPublishedContent AncestorOrSelf(this IPublishedContent content, int maxLevel)
    {
        return AncestorOrSelf(content, GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }

    public static IPublishedContent AncestorOrSelf(this IPublishedContent content, string contentTypeAlias)
    {
        return AncestorOrSelf(content, GetPublishedCache(content), GetNavigationQueryService(content), contentTypeAlias);
    }

    public static T? AncestorOrSelf<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
    {
        return AncestorOrSelf<T>(content, GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, int maxLevel)
    {
        return content.AncestorsOrSelf(GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, string contentTypeAlias)
    {
        return content.Ancestors(GetPublishedCache(content), GetNavigationQueryService(content), contentTypeAlias);
    }

    public static IEnumerable<T> AncestorsOrSelf<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
    {
        return AncestorsOrSelf<T>(content, GetPublishedCache(content), GetNavigationQueryService(content), maxLevel);
    }

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, bool orSelf,
        Func<IPublishedContent, bool>? func)
    {
        return AncestorsOrSelf(content, GetPublishedCache(content), GetNavigationQueryService(content), orSelf, func);
    }

    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        bool andSelf = true) =>
        content.Breadcrumbs(GetPublishedCache(content), GetNavigationQueryService(content), andSelf);

    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        int minLevel,
        bool andSelf = true) =>
        content.Breadcrumbs(GetPublishedCache(content), GetNavigationQueryService(content), minLevel, andSelf);

    public static IEnumerable<IPublishedContent> Breadcrumbs<T>(
        this IPublishedContent content,
        bool andSelf = true)
        where T : class, IPublishedContent=>
        content.Breadcrumbs<T>(GetPublishedCache(content), GetNavigationQueryService(content), andSelf);

    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => Children(content, variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, culture);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        string? culture = null)
        => Children(content, variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);

    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null) =>
        content.Children(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, culture).Where(predicate);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Func<IPublishedContent, bool> predicate,
        string? culture = null) =>
        Children(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            predicate,
            culture);

    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? contentTypeAlias,
        string? culture = null)
    {
        IEnumerable<IPublishedContent> children = contentTypeAlias is not null
            ? GetChildren(GetNavigationQueryService(content), GetPublishedCache(content), content.Key, publishStatusQueryService, variationContextAccessor, contentTypeAlias, culture)
            : [];

        return children.FilterByCulture(culture, variationContextAccessor);
    }

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? contentTypeAlias,
        string? culture = null)
    {
        IPublishStatusQueryService publishStatusQueryService = StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>();
        return ChildrenOfType(content, variationContextAccessor, publishStatusQueryService, contentTypeAlias, culture);
    }

    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Children(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        Children<T>(content, variationContextAccessor, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);

    [Obsolete("This method is no longer used in Umbraco. The method will be removed in Umbraco 17.")]
    public static DataTable ChildrenAsTable(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IPublishedUrlProvider publishedUrlProvider,
        string contentTypeAliasFilter = "",
        string? culture = null)
        => GenerateDataTable(content, variationContextAccessor, GetPublishedCache(content),
            GetNavigationQueryService(content), contentTypeService, mediaTypeService, memberTypeService, publishedUrlProvider, contentTypeAliasFilter, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string docTypeAlias,
        string? culture = null) =>
        parentNodes.SelectMany(x => x.DescendantsOrSelfOfType(
            variationContextAccessor,
            GetPublishedCache(parentNodes.First()),
            GetNavigationQueryService(parentNodes.First()),
            publishStatusQueryService,
            docTypeAlias,
            culture));

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        string docTypeAlias,
        string? culture = null) =>
        DescendantsOrSelfOfType(
            parentNodes,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            docTypeAlias,
            culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        parentNodes.SelectMany(
            x => x.DescendantsOrSelf<T>(
                variationContextAccessor,
                GetPublishedCache(parentNodes.First()),
                GetNavigationQueryService(parentNodes.First()),
                publishStatusQueryService,
                culture));

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantsOrSelf<T>(parentNodes, variationContextAccessor, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);

    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.DescendantsOrSelf(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, false, null, culture);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        Descendants(content, variationContextAccessor, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);


    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) =>
        content.DescendantsOrSelf(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, false, p => p.Level >= level, culture);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null) =>
        Descendants(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);


    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias, string? culture = null) =>
        content.EnumerateDescendantsOrSelfInternal(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            culture,
            false,
            contentTypeAlias);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantsOfType(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);


    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Descendants(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        Descendants<T>(content, variationContextAccessor, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);


    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Descendants(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, level, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        Descendants<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);


    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.DescendantsOrSelf(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, true, null, culture);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        DescendantsOrSelf(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);


    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) =>
        content.DescendantsOrSelf(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            true,
            p => p.Level >= level,
            culture);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null) =>
        DescendantsOrSelf(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        content.EnumerateDescendantsOrSelfInternal(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            culture,
            true,
            contentTypeAlias);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantsOrSelfOfType(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);


    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.DescendantsOrSelf(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantsOrSelf<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);


    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.DescendantsOrSelf(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                level,
                culture).OfType<T>();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantsOrSelf<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);


    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.Children(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        Descendant(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);


    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) => content
        .EnumerateDescendants(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            false,
            culture).FirstOrDefault(x => x.Level == level);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null) =>
        Descendant(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);


    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) => content
            .EnumerateDescendantsOrSelfInternal(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                culture,
                false,
                contentTypeAlias)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantOfType(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);


    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.EnumerateDescendants(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                false,
                culture)
            .FirstOrDefault(x => x is T) as T;

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        Descendant<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);


    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Descendant(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            level,
            culture) as T;

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        Descendant<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);


    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null) => content
        .EnumerateDescendants(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            true,
            culture).FirstOrDefault(x => x.Level == level);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null) => DescendantOrSelf(content, variationContextAccessor, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), level, culture);


    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) => content
            .EnumerateDescendantsOrSelfInternal(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                culture,
                true,
                contentTypeAlias)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null) =>
        DescendantOrSelfOfType(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);


    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.EnumerateDescendants(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                true,
                culture).FirstOrDefault(x => x is T) as T;

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantOrSelf<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);

    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.DescendantOrSelf(variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), publishStatusQueryService, level, culture) as T;

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent =>
        DescendantOrSelf<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            level,
            culture);



    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null) =>
        content.Children(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        FirstChild(content, variationContextAccessor, StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(), culture);


    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null) =>
        content.ChildrenOfType(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            contentTypeAlias,
            culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null) =>
        FirstChildOfType(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            contentTypeAlias,
            culture);


    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null) =>
            content.Children(
                variationContextAccessor,
                GetPublishedCache(content),
                GetNavigationQueryService(content),
                publishStatusQueryService,
                predicate,
                culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(variationContextAccessor, GetPublishedCache(content),
            GetNavigationQueryService(content), predicate, culture)?.FirstOrDefault();


    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Guid uniqueId,
        string? culture = null) => content
        .Children(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            x => x.Key == uniqueId,
            culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Guid uniqueId,
        string? culture = null) =>
        FirstChild(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            uniqueId,
            culture);


    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Children<T>(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        FirstChild<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);


    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent =>
        content.Children<T>(
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            culture)?.FirstOrDefault(predicate);

    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent =>
        FirstChild<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            predicate,
            culture);

    [Obsolete(
        "Please use IPublishedCache and IDocumentNavigationQueryService or IMediaNavigationQueryService directly. This will be removed in a future version of Umbraco")]
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) =>
        Siblings(content, GetPublishedCache(content), GetNavigationQueryService(content), variationContextAccessor, culture);

    [Obsolete(
        "Please use IPublishedCache and IDocumentNavigationQueryService or IMediaNavigationQueryService directly. This will be removed in a future version of Umbraco")]
    public static IEnumerable<IPublishedContent> SiblingsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null) =>
        SiblingsOfType(content, variationContextAccessor,
            GetPublishedCache(content), GetNavigationQueryService(content), contentTypeAlias, culture);

    [Obsolete(
        "Please use IPublishedCache and IDocumentNavigationQueryService or IMediaNavigationQueryService directly. This will be removed in a future version of Umbraco")]
    public static IEnumerable<T> Siblings<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent =>
        Siblings<T>(content, variationContextAccessor, GetPublishedCache(content), GetNavigationQueryService(content), culture);

    [Obsolete(
        "Please use IPublishedCache and IDocumentNavigationQueryService or IMediaNavigationQueryService directly. This will be removed in a future version of Umbraco")]
    public static IEnumerable<IPublishedContent>? SiblingsAndSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null) => SiblingsAndSelf(content, GetPublishedCache(content), GetNavigationQueryService(content), variationContextAccessor, culture);

    [Obsolete(
        "Please use IPublishedCache and IDocumentNavigationQueryService or IMediaNavigationQueryService directly. This will be removed in a future version of Umbraco")]
    public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null) => SiblingsAndSelfOfType(content, variationContextAccessor, GetPublishedCache(content),
        GetNavigationQueryService(content), contentTypeAlias, culture);


    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent =>
        SiblingsAndSelf<T>(
            content,
            variationContextAccessor,
            GetPublishedCache(content),
            GetNavigationQueryService(content),
            publishStatusQueryService,
            culture);

    [Obsolete("Use the overload with IPublishStatusQueryService, scheduled for removal in v17")]
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent => SiblingsAndSelf<T>(
            content,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            culture);


    private static INavigationQueryService GetNavigationQueryService(IPublishedContent content)
    {
        switch (content.ContentType.ItemType)
        {
            case PublishedItemType.Content:
                return StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
            case PublishedItemType.Media:
                return StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>();
            default:
                throw new NotSupportedException("Unsupported content type.");
        }

    }

    private static IPublishedCache GetPublishedCache(IPublishedContent content)
    {
        switch (content.ContentType.ItemType)
        {
            case PublishedItemType.Content:
                return StaticServiceProvider.Instance.GetRequiredService<IPublishedContentCache>();
            case PublishedItemType.Media:
                return StaticServiceProvider.Instance.GetRequiredService<IPublishedMediaCache>();
            default:
                throw new NotSupportedException("Unsupported content type.");
        }
    }

    private static IEnumerable<IPublishedContent> GetChildren(
        INavigationQueryService navigationQueryService,
        IPublishedCache publishedCache,
        Guid parentKey,
        IPublishStatusQueryService publishStatusQueryService,
        IVariationContextAccessor? variationContextAccessor,
        string? contentTypeAlias = null,
        string? culture = null)
    {
        var nodeExists = contentTypeAlias is null
            ? navigationQueryService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> childrenKeys)
            : navigationQueryService.TryGetChildrenKeysOfType(parentKey, contentTypeAlias, out childrenKeys);

        if (nodeExists is false)
        {
            return [];
        }
        // We need to filter what keys are published, as calling the GetById
        // with a non-existing published node, will get cache misses and call the DB
        // making it a very slow operation.

        culture ??= variationContextAccessor?.VariationContext?.Culture ?? string.Empty;

        return childrenKeys
            .Where(x => publishStatusQueryService.IsDocumentPublished(x, culture))
            .Select(publishedCache.GetById)
            .WhereNotNull()
            .OrderBy(x => x.SortOrder);
    }

    private static IEnumerable<IPublishedContent> FilterByCulture(
        this IEnumerable<IPublishedContent> contentNodes,
        string? culture,
        IVariationContextAccessor? variationContextAccessor)
    {
        // Determine the culture if not provided
        culture ??= variationContextAccessor?.VariationContext?.Culture ?? string.Empty;

        return culture == "*"
            ? contentNodes
            : contentNodes.Where(x => x.IsInvariantOrHasCulture(culture));
    }

    private static IEnumerable<IPublishedContent> EnumerateDescendantsOrSelfInternal(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture,
        bool orSelf,
        string? contentTypeAlias = null)
    {
        if (orSelf)
        {
            if (contentTypeAlias is null || content.ContentType.Alias == contentTypeAlias)
            {
                yield return content;
            }
        }

        var nodeExists = contentTypeAlias is null
            ? navigationQueryService.TryGetDescendantsKeys(content.Key, out IEnumerable<Guid> descendantsKeys)
            : navigationQueryService.TryGetDescendantsKeysOfType(content.Key, contentTypeAlias, out descendantsKeys);

        if (nodeExists is false)
        {
            yield break;
        }

        culture ??= variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        IEnumerable<IPublishedContent> descendants = descendantsKeys
            .Where(x => publishStatusQueryService.IsDocumentPublished(x, culture))
            .Select(publishedCache.GetById)
            .WhereNotNull()
            .FilterByCulture(culture, variationContextAccessor);

        foreach (IPublishedContent descendant in descendants)
        {
            yield return descendant;
        }
    }

    private static IEnumerable<IPublishedContent> EnumerateAncestorsOrSelfInternal(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        bool orSelf,
        string? contentTypeAlias = null,
        string? culture = null)
    {
        if (orSelf)
        {
            if (contentTypeAlias is null || content.ContentType.Alias == contentTypeAlias)
            {
                yield return content;
            }
        }

        var nodeExists = contentTypeAlias is null
            ? navigationQueryService.TryGetAncestorsKeys(content.Key, out IEnumerable<Guid> ancestorsKeys)
            : navigationQueryService.TryGetAncestorsKeysOfType(content.Key, contentTypeAlias, out ancestorsKeys);

        if (nodeExists is false)
        {
            yield break;
        }

        culture ??= variationContextAccessor.VariationContext?.Culture ?? string.Empty;
        foreach (Guid ancestorKey in ancestorsKeys)
        {
            if (publishStatusQueryService.IsDocumentPublished(ancestorKey, culture) is false)
            {
                yield break;
            }

            IPublishedContent? ancestor = publishedCache.GetById(ancestorKey);
            if (ancestor is not null)
            {
                yield return ancestor;
            }
        }
    }
}
