// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <c>IPublishedElement</c>.
/// </summary>
public static class PublishedElementExtensions
{
    #region OfTypes

    /// <summary>
    /// Filters published elements by their content type alias.
    /// </summary>
    /// <typeparam name="T">The type of published elements.</typeparam>
    /// <param name="contents">The elements to filter.</param>
    /// <param name="types">The content type aliases to include.</param>
    /// <returns>Elements whose content type alias matches any of the specified types.</returns>
    /// <remarks>
    /// The .OfType&lt;T&gt;() filter is nice when there's only one type.
    /// This method supports filtering with multiple types.
    /// </remarks>
    public static IEnumerable<T> OfTypes<T>(this IEnumerable<T> contents, params string[] types)
        where T : IPublishedElement
    {
        if (types == null || types.Length == 0)
        {
            return Enumerable.Empty<T>();
        }

        return contents.Where(x => types.InvariantContains(x.ContentType.Alias));
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
    public static bool IsComposedOf(this IPublishedElement content, string alias) =>
        content.ContentType.CompositionAliases.InvariantContains(alias);

    #endregion

    #region HasProperty

    /// <summary>
    ///     Gets a value indicating whether the content has a property identified by its alias.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="alias">The property alias.</param>
    /// <returns>A value indicating whether the content has the property identified by the alias.</returns>
    /// <remarks>The content may have a property, and that property may not have a value.</remarks>
    public static bool HasProperty(this IPublishedElement content, string alias) =>
        content.ContentType.GetPropertyType(alias) != null;

    #endregion

    #region HasValue

    /// <summary>
    ///     Gets a value indicating whether the content has a value for a property identified by its alias.
    /// </summary>
    /// <remarks>
    ///     Returns true if <c>GetProperty(alias)</c> is not <c>null</c> and <c>GetProperty(alias).HasValue</c> is
    ///     <c>true</c>.
    /// </remarks>
    public static bool HasValue(this IPublishedElement content, string alias, string? culture = null, string? segment = null)
    {
        IPublishedProperty? prop = content.GetProperty(alias);
        return prop != null && prop.HasValue(culture, segment);
    }

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
    public static bool HasValue(
        this IPublishedElement content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default)
    {
        IPublishedProperty? property = content.GetProperty(alias);

        // if we have a property, and it has a value, return that value
        if (property != null && property.HasValue(culture, segment))
        {
            return true;
        }

        // else let fallback try to get a value
        return publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, null, out object? _);
    }

    #endregion

    #region Value

    /// <summary>
    ///     Gets the value of a content's property identified by its alias.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback">The published value fallback implementation.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
    /// <remarks>
    ///     <para>
    ///         The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering
    ///         content.
    ///     </para>
    ///     <para>
    ///         If no property with the specified alias exists, or if the property has no value, returns
    ///         <paramref name="defaultValue" />.
    ///     </para>
    ///     <para>
    ///         If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the
    ///         converter.
    ///     </para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
    public static object? Value(
        this IPublishedElement content,
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
        if (publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out var value))
        {
            return value;
        }

        // else... if we have a property, at least let the converter return its own
        // vision of 'no value' (could be an empty enumerable) - otherwise, default
        return property?.GetValue(culture, segment);
    }

    #endregion

    #region Value<T>

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
    /// <remarks>
    ///     <para>
    ///         The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering
    ///         content.
    ///     </para>
    ///     <para>
    ///         If no property with the specified alias exists, or if the property has no value, or if it could not be
    ///         converted, returns <c>default(T)</c>.
    ///     </para>
    ///     <para>
    ///         If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the
    ///         converter.
    ///     </para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
    public static T? Value<T>(
        this IPublishedElement content,
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
            return property.Value<T>(publishedValueFallback, culture, segment, fallback);
        }

        // else let fallback try to get a value
        if (publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out T? value))
        {
            return value;
        }

        // else... if we have a property, at least let the converter return its own
        // vision of 'no value' (could be an empty enumerable) - otherwise, default
        return property == null ? default : property.Value<T>(publishedValueFallback, culture, segment, fallback);
    }

    #endregion

    #region ToIndexedArray

    /// <summary>
    /// Converts an enumerable of published elements to an indexed array with positional information.
    /// </summary>
    /// <typeparam name="TContent">The type of published elements.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <returns>An array of indexed items containing positional information.</returns>
    public static IndexedArrayItem<TContent>[] ToIndexedArray<TContent>(this IEnumerable<TContent> source)
        where TContent : class, IPublishedElement
    {
        IndexedArrayItem<TContent>[] set =
            source.Select((content, index) => new IndexedArrayItem<TContent>(content, index)).ToArray();
        foreach (IndexedArrayItem<TContent> setItem in set)
        {
            setItem.TotalCount = set.Length;
        }

        return set;
    }

    #endregion

    #region IsSomething

    /// <summary>
    ///     Gets a value indicating whether the content is visible.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback">The published value fallback implementation.</param>
    /// <returns>A value indicating whether the content is visible.</returns>
    /// <remarks>
    ///     A content is not visible if it has an umbracoNaviHide property with a value of "1". Otherwise,
    ///     the content is visible.
    /// </remarks>
    public static bool IsVisible(this IPublishedElement content, IPublishedValueFallback publishedValueFallback) =>

        // rely on the property converter - will return default bool value, ie false, if property
        // is not defined, or has no value, else will return its value.
        content.Value<bool>(publishedValueFallback, Constants.Conventions.Content.NaviHide) == false;

    #endregion

    #region Culture

    /// <summary>
    ///     Determines whether the content has a culture.
    /// </summary>
    /// <remarks>Culture is case-insensitive.</remarks>
    public static bool HasCulture(this IPublishedElement content, string? culture)
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
    public static bool IsInvariantOrHasCulture(this IPublishedElement content, string culture)
        => !content.ContentType.VariesByCulture() || content.Cultures.ContainsKey(culture ?? string.Empty);

    /// <summary>
    ///     Gets the culture date of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static DateTime CultureDate(this IPublishedElement content, IVariationContextAccessor variationContextAccessor, string? culture = null)
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

    #region IsSomething: misc.

    /// <summary>
    ///     Determines whether the specified content is a specified content type.
    /// </summary>
    /// <param name="content">The content to determine content type of.</param>
    /// <param name="docTypeAlias">The alias of the content type to test against.</param>
    /// <returns>True if the content is of the specified content type; otherwise false.</returns>
    public static bool IsDocumentType(this IPublishedElement content, string docTypeAlias) =>
        content.ContentType.Alias.InvariantEquals(docTypeAlias);

    /// <summary>
    ///     Determines whether the specified content is a specified content type or it's derived types.
    /// </summary>
    /// <param name="content">The content to determine content type of.</param>
    /// <param name="docTypeAlias">The alias of the content type to test against.</param>
    /// <param name="recursive">
    ///     When true, recurses up the content type tree to check inheritance; when false just calls
    ///     IsDocumentType(this IPublishedElement content, string docTypeAlias).
    /// </param>
    /// <returns>True if the content is of the specified content type or a derived content type; otherwise false.</returns>
    public static bool IsDocumentType(this IPublishedElement content, string docTypeAlias, bool recursive)
    {
        if (content.IsDocumentType(docTypeAlias))
        {
            return true;
        }

        return recursive && content.IsComposedOf(docTypeAlias);
    }

    #endregion

    #region IsSomething: equality

    /// <summary>
    /// Determines whether this content item is equal to another content item by comparing their IDs.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item to compare.</param>
    /// <returns><c>true</c> if both content items have the same ID; otherwise, <c>false</c>.</returns>
    public static bool IsEqual(this IPublishedElement content, IPublishedElement other) => content.Id == other.Id;

    /// <summary>
    /// Determines whether this content item is not equal to another content item by comparing their IDs.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item to compare.</param>
    /// <returns><c>true</c> if the content items have different IDs; otherwise, <c>false</c>.</returns>
    public static bool IsNotEqual(this IPublishedElement content, IPublishedElement other) =>
        content.IsEqual(other) == false;

    #endregion

    #region Writer and creator

    /// <summary>
    /// Gets the name of the user who created the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="userService">The user service.</param>
    /// <returns>The name of the creator, or an empty string if not found.</returns>
    public static string GetCreatorName(this IPublishedElement content, IUserService userService)
    {
        IProfile? user = userService.GetProfileById(content.CreatorId);
        return user?.Name ?? string.Empty;
    }

    /// <summary>
    /// Gets the name of the user who last updated the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="userService">The user service.</param>
    /// <returns>The name of the writer, or an empty string if not found.</returns>
    public static string GetWriterName(this IPublishedElement content, IUserService userService)
    {
        IProfile? user = userService.GetProfileById(content.WriterId);
        return user?.Name ?? string.Empty;
    }

    #endregion

    #region MediaUrl

    /// <summary>
    ///     Gets the url for a media.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="publishedUrlProvider">The published url provider.</param>
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
        IPublishedUrlProvider publishedUrlProvider,
        string? culture = null,
        UrlMode mode = UrlMode.Default,
        string propertyAlias = Constants.Conventions.Media.File)
    {
        if (publishedUrlProvider == null)
        {
            throw new ArgumentNullException(nameof(publishedUrlProvider));
        }

        return publishedUrlProvider.GetMediaUrl(content, mode, culture, propertyAlias);
    }

    #endregion
}
