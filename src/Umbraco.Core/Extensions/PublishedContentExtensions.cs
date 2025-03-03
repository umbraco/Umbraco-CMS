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
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The parent of content, of the given content type, else null.</returns>
    public static T? Parent<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.GetParent(navigationQueryService, publishedStatusFilteringService) as T;
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Parent<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.Parent<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    private static IPublishedContent? GetParent(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
    {
        if (navigationQueryService.TryGetParentKey(content.Key, out Guid? parentKey) is false)
        {
            throw new KeyNotFoundException($"Content with key '{content.Key}' was not found in the in-memory navigation structure.");
        }

        // parent key is null if content is at root
        return parentKey.HasValue
            ? publishedStatusFilteringService.FilterAvailable([parentKey.Value], null).FirstOrDefault()
            : null;
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
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The ancestors of the content, in down-top order.</returns>
    /// <remarks>Does not consider the content itself.</remarks>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, false, null);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the ancestors of the content, at a level lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The ancestors of the content, at a level lesser or equal to the specified level, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Only content that are "high enough" in the tree are returned.</remarks>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        => content.AncestorsOrSelf(
            navigationQueryService,
            publishedStatusFilteringService,
            false,
            n => n.Level <= maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Gets the ancestors of the content, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.EnumerateAncestorsOrSelfInternal(
            navigationQueryService,
            publishedStatusFilteringService,
            false,
            contentTypeAlias);
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    /// <summary>
    ///     Gets the ancestors of the content, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
    /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
        => content.Ancestors(navigationQueryService, publishedStatusFilteringService).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent
        => content.Ancestors<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.Ancestors<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the ancestors of the content, at a level lesser or equal to a specified level, and of a specified content
    ///     type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedStatusFilteringService"></param>
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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestors(navigationQueryService, publishedStatusFilteringService, maxLevel).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestors<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestors<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Gets the content and its ancestors.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The content and its ancestors, in down-top order.</returns>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, true, null);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the content and its ancestors, at a level lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        => content.AncestorsOrSelf(
            navigationQueryService,
            publishedStatusFilteringService,
            true,
            n => n.Level <= maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Gets the content and its ancestors, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
    /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.EnumerateAncestorsOrSelfInternal(
            navigationQueryService,
            publishedStatusFilteringService,
            true,
            contentTypeAlias);
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    /// <summary>
    ///     Gets the content and its ancestors, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
    /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the content and its ancestor, at a lever lesser or equal to a specified level, and of a specified content
    ///     type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     The content and its ancestors, at a level lesser or equal to the specified level, and of the specified
    ///     content type, in down-top order.
    /// </returns>
    /// <remarks>May or may not begin with the content itself, depending on its level and content type.</remarks>
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, maxLevel).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Gets the ancestor of the content, ie its parent.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The ancestor of the content.</returns>
    /// <remarks>This method is here for consistency purposes but does not make much sense.</remarks>
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => content.GetParent(navigationQueryService, publishedStatusFilteringService);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        => content.Ancestor(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the nearest ancestor of the content, at a lever lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, at a level lesser or equal to the specified level.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        => content
            .EnumerateAncestors(navigationQueryService, publishedStatusFilteringService, false)
            .FirstOrDefault(x => x.Level <= maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        => content.Ancestor(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        => content.Ancestor(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Gets the nearest ancestor of the content, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content
            .EnumerateAncestorsOrSelfInternal(
                navigationQueryService,
                publishedStatusFilteringService,
                false,
                contentTypeAlias)
            .FirstOrDefault();
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
        => content.Ancestor(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Ancestor(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias)
        => content.Ancestor(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    /// <summary>
    ///     Gets the nearest ancestor of the content, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
    /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
    public static T? Ancestor<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
        => content.Ancestors<T>(navigationQueryService, publishedStatusFilteringService).FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent
        => content.Ancestor<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.Ancestor<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the nearest ancestor of the content, at the specified level and of the specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The ancestor of the content, at the specified level and of the specified content type.</returns>
    /// <remarks>
    ///     Does not consider the content itself. If the ancestor at the specified level is
    ///     not of the specified type, returns <c>null</c>.
    /// </remarks>
    public static T? Ancestor<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestors<T>(navigationQueryService, publishedStatusFilteringService, maxLevel).FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestor<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Ancestor<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestor<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

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
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, at a level lesser or equal to the specified level.</returns>
    /// <remarks>May or may not return the content itself depending on its level. May return <c>null</c>.</remarks>
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        => content
            .EnumerateAncestors(navigationQueryService, publishedStatusFilteringService, true)
            .FirstOrDefault(x => x.Level <= maxLevel) ?? content;

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        => content.AncestorOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        => content.AncestorOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Gets the content or its nearest ancestor, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="contentTypeAlias">The content type.</param>
    /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
    /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content
            .EnumerateAncestorsOrSelfInternal(
                navigationQueryService,
                publishedStatusFilteringService,
                true,
                contentTypeAlias)
            .FirstOrDefault() ?? content;
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias)
        => content.AncestorOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent AncestorOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias)
        => content.AncestorOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    /// <summary>
    ///     Gets the content or its nearest ancestor, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
    /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent =>
        content.AncestorsOrSelf<T>(navigationQueryService, publishedStatusFilteringService).FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent
        => content.AncestorOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.AncestorOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the content or its nearest ancestor, at a lever lesser or equal to a specified level, and of a specified
    ///     content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns></returns>
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(navigationQueryService, publishedStatusFilteringService, maxLevel).FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? AncestorOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool orSelf,
        Func<IPublishedContent, bool>? func)
    {
        IEnumerable<IPublishedContent> ancestorsOrSelf = content.EnumerateAncestors(
            navigationQueryService,
            publishedStatusFilteringService,
            orSelf);
        return func == null ? ancestorsOrSelf : ancestorsOrSelf.Where(func);
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        bool orSelf,
        Func<IPublishedContent, bool>? func)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), orSelf, func);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        bool orSelf,
        Func<IPublishedContent, bool>? func)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), orSelf, func);

    private static IEnumerable<IPublishedContent> EnumerateAncestors(
        this IPublishedContent? content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool orSelf)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.EnumerateAncestorsOrSelfInternal(navigationQueryService, publishedStatusFilteringService, orSelf);
    }

    #endregion

    #region Axes: breadcrumbs

    /// <summary>
    ///     Gets the breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="andSelf">Indicates whether the specified content should be included.</param>
    /// <returns>
    ///     The breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" />.
    /// </returns>
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool andSelf = true) =>
        content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, andSelf, null).Reverse();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        bool andSelf = true)
        => content.Breadcrumbs(navigationQueryService, GetPublishedStatusFilteringService(content), andSelf);

    /// <summary>
    ///     Gets the breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level
    ///     higher or equal to <paramref name="minLevel" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="minLevel">The minimum level.</param>
    /// <param name="andSelf">Indicates whether the specified content should be included.</param>
    /// <returns>
    ///     The breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level higher
    ///     or equal to <paramref name="minLevel" />.
    /// </returns>
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int minLevel,
        bool andSelf = true)
        => content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, andSelf, n => n.Level >= minLevel).Reverse();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int minLevel,
        bool andSelf = true)
        => content.Breadcrumbs(navigationQueryService, GetPublishedStatusFilteringService(content), minLevel, andSelf);

    /// <summary>
    ///     Gets the breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level
    ///     higher or equal to the specified root content type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The root content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="andSelf">Indicates whether the specified content should be included.</param>
    /// <returns>
    ///     The breadcrumbs (ancestors and self, top to bottom) for the specified <paramref name="content" /> at a level higher
    ///     or equal to the specified root content type <typeparamref name="T" />.
    /// </returns>
    public static IEnumerable<IPublishedContent> Breadcrumbs<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool andSelf = true)
        where T : class, IPublishedContent
        => content
            .AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, andSelf, null)
            .TakeWhile(n => n is T)
            .Reverse();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Breadcrumbs<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        bool andSelf = true)
        where T : class, IPublishedContent
        => content.Breadcrumbs<T>(navigationQueryService, GetPublishedStatusFilteringService(content), andSelf);

    #endregion

    #region Axes: descendants, descendants-or-self

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string docTypeAlias,
        string? culture = null)
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelfOfType(
            navigationQueryService,
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            docTypeAlias,
            culture);
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string docTypeAlias,
        string? culture = null)
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelfOfType(
            navigationQueryService,
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            docTypeAlias,
            culture);
    }

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelf<T>(
            navigationQueryService,
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            culture);
    }

    [Obsolete("Use the overload with IPublishStatusQueryService instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelf<T>(
            navigationQueryService,
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            culture);
    }


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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => content.DescendantsOrSelf(navigationQueryService, publishedStatusFilteringService, false, null, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.Descendants(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        => content.Descendants(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        => content.DescendantsOrSelf(
            navigationQueryService,
            publishedStatusFilteringService,
            false,
            p => p.Level >= level,
            culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.Descendants(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        => content.Descendants(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias,
        string? culture = null)
        => content.EnumerateDescendantsOrSelfInternal(
            navigationQueryService,
            publishedStatusFilteringService,
            culture,
            false,
            contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants(navigationQueryService, publishedStatusFilteringService, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants(navigationQueryService, publishedStatusFilteringService, level, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => content.DescendantsOrSelf(navigationQueryService, publishedStatusFilteringService, true, null, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.DescendantsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        => content.DescendantsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        => content.DescendantsOrSelf(navigationQueryService, publishedStatusFilteringService, true, p => p.Level >= level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.DescendantsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        => content.DescendantsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias,
        string? culture = null)
        => content.EnumerateDescendantsOrSelfInternal(
            navigationQueryService,
            publishedStatusFilteringService,
            culture,
            true,
            contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOrSelfOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOrSelfOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf(navigationQueryService, publishedStatusFilteringService, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf(navigationQueryService, publishedStatusFilteringService, level, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => content.Children(navigationQueryService, publishedStatusFilteringService, culture)?.FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.Descendant(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        => content.Descendant(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        => content
            .EnumerateDescendants(navigationQueryService, publishedStatusFilteringService, false, culture)
            .FirstOrDefault(x => x.Level == level);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.Descendant(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        => content.Descendant(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias,
        string? culture = null)
        => content
            .EnumerateDescendantsOrSelfInternal(
                navigationQueryService,
                publishedStatusFilteringService,
                culture,
                false,
                contentTypeAlias)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    public static T? Descendant<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content
            .EnumerateDescendants(navigationQueryService, publishedStatusFilteringService, false, culture)
            .FirstOrDefault(x => x is T) as T;

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static T? Descendant<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant(navigationQueryService, publishedStatusFilteringService, level, culture) as T;

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    public static IPublishedContent DescendantOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => content.EnumerateDescendants(
                navigationQueryService,
                publishedStatusFilteringService,
                true,
                culture)
            .FirstOrDefault() ??
        content;

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.DescendantOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.DescendantOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        => content
            .EnumerateDescendants(navigationQueryService, publishedStatusFilteringService, true, culture)
            .FirstOrDefault(x => x.Level == level);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.DescendantOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);


    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        => content.DescendantOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias,
        string? culture = null)
        => content
            .EnumerateDescendantsOrSelfInternal(
                navigationQueryService,
                publishedStatusFilteringService,
                culture,
                true,
                contentTypeAlias)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOrSelfOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOrSelfOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content
            .EnumerateDescendants(navigationQueryService, publishedStatusFilteringService, true, culture)
            .FirstOrDefault(x => x is T) as T;

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf(navigationQueryService, publishedStatusFilteringService, level, culture) as T;

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), level, culture);

    private static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool orSelf,
        Func<IPublishedContent, bool>? func,
        string? culture = null)
        => content
            .EnumerateDescendants(navigationQueryService, publishedStatusFilteringService, orSelf, culture)
            .Where(x => func == null || func(x));

    private static IEnumerable<IPublishedContent> EnumerateDescendants(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool orSelf,
        string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        foreach (IPublishedContent desc in content.EnumerateDescendantsOrSelfInternal(
                     navigationQueryService,
                     publishedStatusFilteringService,
                     culture,
                     orSelf))
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
    /// <param name="navigationQueryService"></param>
    /// <param name="publishedStatusFilteringService"></param>
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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => GetChildren(navigationQueryService, publishedStatusFilteringService, content.Key, null, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.Children(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        => content.Children(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    /// <summary>
    ///     Gets the children of the content, filtered by a predicate.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishedStatusFilteringService"></param>
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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(navigationQueryService, publishedStatusFilteringService, culture).Where(predicate);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(navigationQueryService, GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(navigationQueryService, GetPublishedStatusFilteringService(content), predicate, culture);

    /// <summary>
    ///     Gets the children of the content, of any of the specified types.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The children of the content, of any of the specified types.</returns>
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? contentTypeAlias,
        string? culture = null)
        => contentTypeAlias is not null
            ? GetChildren(navigationQueryService, publishedStatusFilteringService, content.Key, contentTypeAlias, culture)
            : [];

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? contentTypeAlias,
        string? culture = null)
        => content.ChildrenOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? contentTypeAlias,
        string? culture = null)
        => content.ChildrenOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Children(navigationQueryService, publishedStatusFilteringService, culture).OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Children<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Children<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => content
            .Children(navigationQueryService, publishedStatusFilteringService, culture)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.FirstChild(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        => content.FirstChild(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    /// <summary>
    ///     Gets the first child of the content, of a given content type.
    /// </summary>
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias,
        string? culture = null)
        => content
            .ChildrenOfType(navigationQueryService, publishedStatusFilteringService, contentTypeAlias, culture)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.FirstChildOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.FirstChildOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content
            .Children(navigationQueryService, publishedStatusFilteringService, predicate, culture)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.FirstChild(navigationQueryService, GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.FirstChild(navigationQueryService, GetPublishedStatusFilteringService(content), predicate, culture);

    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Guid uniqueId,
        string? culture = null)
        => content
            .Children(navigationQueryService, publishedStatusFilteringService, x => x.Key == uniqueId, culture)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Guid uniqueId,
        string? culture = null)
        => content.FirstChild(navigationQueryService, GetPublishedStatusFilteringService(content), uniqueId, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Guid uniqueId,
        string? culture = null)
        => content.FirstChild(navigationQueryService, GetPublishedStatusFilteringService(content), uniqueId, culture);

    public static T? FirstChild<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content
            .Children<T>(navigationQueryService, publishedStatusFilteringService, culture)
            .FirstOrDefault();

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    public static T? FirstChild<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent
        => content
            .Children<T>(navigationQueryService, publishedStatusFilteringService, culture)
            .FirstOrDefault(predicate);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(navigationQueryService, GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService instead, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(navigationQueryService, GetPublishedStatusFilteringService(content), predicate, culture);

    #endregion

    #region Axes: siblings

    /// <summary>
    ///     Gets the siblings of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The navigation service</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => content
            .SiblingsAndSelf(navigationQueryService, publishedStatusFilteringService, culture)
            .Where(x => x.Id != content.Id);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.Siblings(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.Siblings(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The siblings of the content, of the given content type.</returns>
    /// <remarks>
    ///     <para>Note that in V7 this method also return the content node self.</para>
    /// </remarks>
    public static IEnumerable<IPublishedContent> SiblingsOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias,
        string? culture = null)
        => content
            .SiblingsAndSelfOfType(navigationQueryService, publishedStatusFilteringService, contentTypeAlias, culture)
            .Where(x => x.Id != content.Id);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    /// <summary>
    ///     Gets the siblings of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishedStatusFilteringService"></param>
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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content
            .SiblingsAndSelf<T>(navigationQueryService, publishedStatusFilteringService, culture)
            .Where(x => x.Id != content.Id);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Siblings<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Siblings<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Siblings<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Siblings<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The navigation service.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content including the node itself.</returns>
    public static IEnumerable<IPublishedContent> SiblingsAndSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        => content.SiblingsAndSelfInternal(navigationQueryService, publishedStatusFilteringService, null, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsAndSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.SiblingsAndSelf(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsAndSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.SiblingsAndSelf(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsAndSelfInternal(navigationQueryService, publishedStatusFilteringService, contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsAndSelfOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsAndSelfOfType(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    /// <summary>
    ///     Gets the siblings of the content including the node itself to indicate the position, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService"></param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="culture">
    ///     The specific culture to filter for. If null is used the current culture is used. (Default is
    ///     null)
    /// </param>
    /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => content
            .SiblingsAndSelfInternal(navigationQueryService, publishedStatusFilteringService, null, culture)
            .OfType<T>();

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.SiblingsAndSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.SiblingsAndSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content), culture);

    #endregion

    #region Axes: custom

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => content.AncestorOrSelf(navigationQueryService, publishedStatusFilteringService, 1);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent Root(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        => content.Root(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent Root(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        => content.Root(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Gets the root content (ancestor or self at level 1) for the specified <paramref name="content" /> if it's of the
    ///     specified content type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
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
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
        => content.AncestorOrSelf<T>(navigationQueryService, publishedStatusFilteringService, 1);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Root<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService)
        where T : class, IPublishedContent
        => content.Root<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Root<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.Root<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

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
        => content.Ancestor(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IPublishedContent? Ancestor(this IPublishedContent content, string contentTypeAlias)
        => content.Ancestor(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias);

    public static T? Ancestor<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestor<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, int maxLevel)
        => content.Ancestors(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, string contentTypeAlias)
        => content.Ancestors(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias);

    public static IEnumerable<T> Ancestors<T>(this IPublishedContent content)
        where T : class, IPublishedContent
        => content.Ancestors<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content));

    public static IEnumerable<T> Ancestors<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
        => content.Ancestors<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IPublishedContent AncestorOrSelf(this IPublishedContent content, int maxLevel)
        => content.AncestorOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IPublishedContent AncestorOrSelf(this IPublishedContent content, string contentTypeAlias)
        => content.AncestorOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias);

    public static T? AncestorOrSelf<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, int maxLevel)
        => content.AncestorsOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, string contentTypeAlias)
        => content.Ancestors(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias);

    public static IEnumerable<T> AncestorsOrSelf<T>(this IPublishedContent content, int maxLevel)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), maxLevel);

    public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, bool orSelf, Func<IPublishedContent, bool>? func)
        => content.AncestorsOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), orSelf, func);

    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        bool andSelf = true) =>
        content.Breadcrumbs(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), andSelf);

    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        int minLevel,
        bool andSelf = true) =>
        content.Breadcrumbs(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), minLevel, andSelf);

    public static IEnumerable<IPublishedContent> Breadcrumbs<T>(
        this IPublishedContent content,
        bool andSelf = true)
        where T : class, IPublishedContent=>
        content.Breadcrumbs<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), andSelf);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.Children(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        string? culture = null)
        => content.Children(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.Children(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? contentTypeAlias,
        string? culture = null)
        => content.ChildrenOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> ChildrenOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? contentTypeAlias,
        string? culture = null)
        => content.ChildrenOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Children<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Children<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);


    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string docTypeAlias,
        string? culture = null)
        => parentNodes.SelectMany(x => x.DescendantsOrSelfOfType(
            navigationQueryService,
            publishedStatusFilteringService,
            docTypeAlias,
            culture));

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string docTypeAlias,
        string? culture = null)
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelfOfType(
            GetNavigationQueryService(parentNodesAsArray.First()),
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            docTypeAlias,
            culture);
    }

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        string docTypeAlias,
        string? culture = null)
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelfOfType(
            GetNavigationQueryService(parentNodesAsArray.First()),
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            docTypeAlias,
            culture);
    }

    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? culture = null)
        where T : class, IPublishedContent
        => parentNodes.SelectMany(x => x.DescendantsOrSelf<T>(
            navigationQueryService,
            publishedStatusFilteringService,
            culture));

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelf<T>(
            GetNavigationQueryService(parentNodesAsArray.First()),
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            culture);
    }

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IEnumerable<IPublishedContent> parentNodes,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
    {
        IPublishedContent[] parentNodesAsArray = parentNodes as IPublishedContent[] ?? parentNodes.ToArray();
        return parentNodesAsArray.DescendantsOrSelf<T>(
            GetNavigationQueryService(parentNodesAsArray.First()),
            GetPublishedStatusFilteringService(parentNodesAsArray.First()),
            culture);
    }

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.Descendants(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.Descendants(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.Descendants(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        => content.Descendants(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Descendants<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendants<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.DescendantsOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.DescendantsOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.DescendantsOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        => content.DescendantsOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOrSelfOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantsOrSelfOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> DescendantsOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantsOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.Descendant(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.Descendant(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.Descendant(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? Descendant(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        => content.Descendant(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Descendant<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Descendant<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        => content.DescendantOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        => content.DescendantOrSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOrSelfOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? DescendantOrSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null)
        => content.DescendantOrSelfOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? DescendantOrSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        int level,
        string? culture = null)
        where T : class, IPublishedContent
        => content.DescendantOrSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), level, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        => content.FirstChild(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.FirstChild(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string contentTypeAlias,
        string? culture = null)
        => content.FirstChildOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChildOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null)
        => content.FirstChildOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.FirstChild(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Func<IPublishedContent, bool> predicate,
        string? culture = null)
        => content.FirstChild(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Guid uniqueId,
        string? culture = null)
        => content.FirstChild(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), uniqueId, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Guid uniqueId,
        string? culture = null)
        => content.FirstChild(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), uniqueId, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? FirstChild<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        Func<T, bool> predicate,
        string? culture = null)
        where T : class, IPublishedContent
        => content.FirstChild<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), predicate, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.Siblings(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Siblings<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.Siblings<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent>? SiblingsAndSelf(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        => content.SiblingsAndSelf(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string contentTypeAlias,
        string? culture = null)
        => content.SiblingsAndSelfOfType(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), contentTypeAlias, culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        string? culture = null)
        where T : class, IPublishedContent
        => content.SiblingsAndSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    [Obsolete("Use the overload with INavigationQueryService and IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> SiblingsAndSelf<T>(
        this IPublishedContent content,
        IVariationContextAccessor variationContextAccessor,
        string? culture = null)
        where T : class, IPublishedContent
        => content.SiblingsAndSelf<T>(GetNavigationQueryService(content), GetPublishedStatusFilteringService(content), culture);

    private static INavigationQueryService GetNavigationQueryService(IPublishedContent content)
    {
        switch (content.ItemType)
        {
            case PublishedItemType.Content:
                return StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
            case PublishedItemType.Media:
                return StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>();
            default:
                throw new NotSupportedException("Unsupported content type.");
        }
    }

    private static IPublishedStatusFilteringService GetPublishedStatusFilteringService(IPublishedContent content)
    {
        switch (content.ItemType)
        {
            case PublishedItemType.Content:
                return StaticServiceProvider.Instance.GetRequiredService<IPublishedContentStatusFilteringService>();
            case PublishedItemType.Media:
                return StaticServiceProvider.Instance.GetRequiredService<IPublishedMediaStatusFilteringService>();
            default:
                throw new NotSupportedException("Unsupported content type.");
        }
    }

    private static IEnumerable<IPublishedContent> GetChildren(
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Guid parentKey,
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

        return publishedStatusFilteringService
            .FilterAvailable(childrenKeys, culture)
            .OrderBy(x => x.SortOrder);
    }

    private static IEnumerable<IPublishedContent> EnumerateDescendantsOrSelfInternal(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
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

        IEnumerable<IPublishedContent> descendants = publishedStatusFilteringService
            .FilterAvailable(descendantsKeys, culture);

        foreach (IPublishedContent descendant in descendants)
        {
            yield return descendant;
        }
    }

    private static IEnumerable<IPublishedContent> EnumerateAncestorsOrSelfInternal(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
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

        IEnumerable<IPublishedContent> ancestors = publishedStatusFilteringService.FilterAvailable(ancestorsKeys, culture);
        foreach (IPublishedContent ancestor in ancestors)
        {
            yield return ancestor;
        }
    }

    private static IEnumerable<IPublishedContent> SiblingsAndSelfInternal(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string? contentTypeAlias,
        string? culture)
    {
        if (navigationQueryService.TryGetParentKey(content.Key, out Guid? parentKey) is false)
        {
            return [];
        }

        if (parentKey.HasValue)
        {
            IEnumerable<Guid> childrenKeys;
            var foundChildrenKeys = contentTypeAlias is null
                ? navigationQueryService.TryGetChildrenKeys(parentKey.Value, out childrenKeys)
                : navigationQueryService.TryGetChildrenKeysOfType(parentKey.Value, contentTypeAlias, out childrenKeys);

            return foundChildrenKeys
                ? publishedStatusFilteringService.FilterAvailable(childrenKeys, culture)
                : [];
        }

        IEnumerable<Guid> rootKeys;
        var foundRootKeys = contentTypeAlias is null
            ? navigationQueryService.TryGetRootKeys(out rootKeys)
            : navigationQueryService.TryGetRootKeysOfType(contentTypeAlias, out rootKeys);

        if (foundRootKeys)
        {
            IEnumerable<Guid> rootKeysArray = rootKeys as Guid[] ?? rootKeys.ToArray();
            return rootKeysArray.Contains(content.Key)
                ? publishedStatusFilteringService.FilterAvailable(rootKeysArray, culture)
                : [];
        }

        return [];
    }
}
