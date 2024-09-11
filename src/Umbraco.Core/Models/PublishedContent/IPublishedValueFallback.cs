namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Provides a fallback strategy for getting <see cref="IPublishedElement" /> values.
/// </summary>
public interface IPublishedValueFallback
{
    /// <summary>
    ///     Tries to get a fallback value for a property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="fallback">A fallback strategy.</param>
    /// <param name="defaultValue">An optional default value.</param>
    /// <param name="value">The fallback value.</param>
    /// <returns>A value indicating whether a fallback value could be provided.</returns>
    /// <remarks>
    ///     <para>
    ///         This method is called whenever property.Value(culture, segment, defaultValue) is called, and
    ///         property.HasValue(culture, segment) is false.
    ///     </para>
    ///     <para>It can only fallback at property level (no recurse).</para>
    ///     <para>
    ///         At property level, property.GetValue() does *not* implement fallback, and one has to
    ///         get property.Value() or property.Value{T}() to trigger fallback.
    ///     </para>
    ///     <para>
    ///         Note that <paramref name="culture" /> and <paramref name="segment" /> may not be contextualized,
    ///         so the variant context should be used to contextualize them (see our default implementation in
    ///         the web project.
    ///     </para>
    /// </remarks>
    bool TryGetValue(IPublishedProperty property, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value);

    /// <summary>
    ///     Tries to get a fallback value for a property.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="fallback">A fallback strategy.</param>
    /// <param name="defaultValue">An optional default value.</param>
    /// <param name="value">The fallback value.</param>
    /// <returns>A value indicating whether a fallback value could be provided.</returns>
    /// <remarks>
    ///     <para>
    ///         This method is called whenever property.Value{T}(culture, segment, defaultValue) is called, and
    ///         property.HasValue(culture, segment) is false.
    ///     </para>
    ///     <para>It can only fallback at property level (no recurse).</para>
    ///     <para>
    ///         At property level, property.GetValue() does *not* implement fallback, and one has to
    ///         get property.Value() or property.Value{T}() to trigger fallback.
    ///     </para>
    /// </remarks>
    bool TryGetValue<T>(IPublishedProperty property, string? culture, string? segment, Fallback fallback, T? defaultValue, out T? value);

    /// <summary>
    ///     Tries to get a fallback value for a published element property.
    /// </summary>
    /// <param name="content">The published element.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="fallback">A fallback strategy.</param>
    /// <param name="defaultValue">An optional default value.</param>
    /// <param name="value">The fallback value.</param>
    /// <returns>A value indicating whether a fallback value could be provided.</returns>
    /// <remarks>
    ///     <para>
    ///         This method is called whenever getting the property value for the specified alias, culture and
    ///         segment, either returned no property at all, or a property with HasValue(culture, segment) being false.
    ///     </para>
    ///     <para>It can only fallback at element level (no recurse).</para>
    /// </remarks>
    bool TryGetValue(IPublishedElement content, string alias, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value);

    /// <summary>
    ///     Tries to get a fallback value for a published element property.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="content">The published element.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="fallback">A fallback strategy.</param>
    /// <param name="defaultValue">An optional default value.</param>
    /// <param name="value">The fallback value.</param>
    /// <returns>A value indicating whether a fallback value could be provided.</returns>
    /// <remarks>
    ///     <para>
    ///         This method is called whenever getting the property value for the specified alias, culture and
    ///         segment, either returned no property at all, or a property with HasValue(culture, segment) being false.
    ///     </para>
    ///     <para>It can only fallback at element level (no recurse).</para>
    /// </remarks>
    bool TryGetValue<T>(IPublishedElement content, string alias, string? culture, string? segment, Fallback fallback, T? defaultValue, out T? value);

    /// <summary>
    ///     Tries to get a fallback value for a published content property.
    /// </summary>
    /// <param name="content">The published element.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="fallback">A fallback strategy.</param>
    /// <param name="defaultValue">An optional default value.</param>
    /// <param name="value">The fallback value.</param>
    /// <param name="noValueProperty">The property that does not have a value.</param>
    /// <returns>A value indicating whether a fallback value could be provided.</returns>
    /// <remarks>
    ///     <para>
    ///         This method is called whenever getting the property value for the specified alias, culture and
    ///         segment, either returned no property at all, or a property with HasValue(culture, segment) being false.
    ///     </para>
    ///     <para>
    ///         In an <see cref="IPublishedContent" />, because walking up the tree is possible, the content itself may not
    ///         even
    ///         have a property with the specified alias, but such a property may exist up in the tree. The
    ///         <paramref name="noValueProperty" />
    ///         parameter is used to return a property with no value. That can then be used to invoke a converter and get the
    ///         converter's interpretation of "no value".
    ///     </para>
    /// </remarks>
    bool TryGetValue(
        IPublishedContent content,
        string alias,
        string? culture,
        string? segment,
        Fallback fallback,
        object? defaultValue,
        out object? value,
        out IPublishedProperty? noValueProperty);

    /// <summary>
    ///     Tries to get a fallback value for a published content property.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="content">The published element.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="fallback">A fallback strategy.</param>
    /// <param name="defaultValue">An optional default value.</param>
    /// <param name="value">The fallback value.</param>
    /// <param name="noValueProperty">The property that does not have a value.</param>
    /// <returns>A value indicating whether a fallback value could be provided.</returns>
    /// <remarks>
    ///     <para>
    ///         This method is called whenever getting the property value for the specified alias, culture and
    ///         segment, either returned no property at all, or a property with HasValue(culture, segment) being false.
    ///     </para>
    ///     <para>
    ///         In an <see cref="IPublishedContent" />, because walking up the tree is possible, the content itself may not
    ///         even
    ///         have a property with the specified alias, but such a property may exist up in the tree. The
    ///         <paramref name="noValueProperty" />
    ///         parameter is used to return a property with no value. That can then be used to invoke a converter and get the
    ///         converter's interpretation of "no value".
    ///     </para>
    /// </remarks>
    bool TryGetValue<T>(
        IPublishedContent content,
        string alias,
        string? culture,
        string? segment,
        Fallback fallback,
        T defaultValue,
        out T? value,
        out IPublishedProperty? noValueProperty);
}
