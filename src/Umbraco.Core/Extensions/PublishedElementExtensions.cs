// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <c>IPublishedElement</c>.
/// </summary>
public static class PublishedElementExtensions
{
    #region OfTypes

    // the .OfType<T>() filter is nice when there's only one type
    // this is to support filtering with multiple types
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
            return property.Value<T>(publishedValueFallback, culture, segment);
        }

        // else let fallback try to get a value
        if (publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out T? value))
        {
            return value;
        }

        // else... if we have a property, at least let the converter return its own
        // vision of 'no value' (could be an empty enumerable) - otherwise, default
        return property == null ? default : property.Value<T>(publishedValueFallback, culture, segment);
    }

    #endregion

    #region ToIndexedArray

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
