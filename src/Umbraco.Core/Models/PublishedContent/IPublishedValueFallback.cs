using Umbraco.Core.Composing;

namespace Umbraco.Core.Models.PublishedContent
{
    // fixme document
    // fixme add values?
    public enum PublishedValueFallbackPriority
    {
        RecursiveTree,
        FallbackLanguage
    }

    /// <summary>
    /// Provides a fallback strategy for getting <see cref="IPublishedElement"/> values.
    /// </summary>
    // fixme - IPublishedValueFallback is still WorkInProgress
    // todo - properly document methods, etc
    // todo - understand caching vs fallback (recurse etc)
    public interface IPublishedValueFallback
    {
        /// <summary>
        /// Gets a fallback value for a property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="culture">The requested culture.</param>
        /// <param name="segment">The requested segment.</param>
        /// <param name="defaultValue">An optional default value.</param>
        /// <returns>A fallback value, or null.</returns>
        /// <remarks>
        /// <para>This method is called whenever property.Value(culture, segment, defaultValue) is called, and
        /// property.HasValue(culture, segment) is false.</para>
        /// <para>It can only fallback at property level (no recurse).</para>
        /// <para>At property level, property.GetValue() does *not* implement fallback, and one has to
        /// get property.Value() or property.Value{T}() to trigger fallback.</para>
        /// </remarks>
        object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue);

        /// <summary>
        /// Gets a fallback value for a property.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="culture">The requested culture.</param>
        /// <param name="segment">The requested segment.</param>
        /// <param name="defaultValue">An optional default value.</param>
        /// <returns>A fallback value, or null.</returns>
        /// <remarks>
        /// <para>This method is called whenever property.Value{T}(culture, segment, defaultValue) is called, and
        /// property.HasValue(culture, segment) is false.</para>
        /// <para>It can only fallback at property level (no recurse).</para>
        /// <para>At property level, property.GetValue() does *not* implement fallback, and one has to
        /// get property.Value() or property.Value{T}() to trigger fallback.</para>
        /// </remarks>
        T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue);

        /// <summary>
        /// Gets a fallback value for a published element property.
        /// </summary>
        /// <param name="content">The published element.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The requested culture.</param>
        /// <param name="segment">The requested segment.</param>
        /// <param name="defaultValue">An optional default value.</param>
        /// <returns>A fallback value, or null.</returns>
        /// <remarks>
        /// <para>This method is called whenever getting the property value for the specified alias, culture and
        /// segment, either returned no property at all, or a property with HasValue(culture, segment) being false.</para>
        /// <para>It can only fallback at element level (no recurse).</para>
        /// </remarks>
        object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue);

        /// <summary>
        /// Gets a fallback value for a published element property.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="content">The published element.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The requested culture.</param>
        /// <param name="segment">The requested segment.</param>
        /// <param name="defaultValue">An optional default value.</param>
        /// <returns>A fallback value, or null.</returns>
        /// <remarks>
        /// <para>This method is called whenever getting the property value for the specified alias, culture and
        /// segment, either returned no property at all, or a property with HasValue(culture, segment) being false.</para>
        /// <para>It can only fallback at element level (no recurse).</para>
        /// </remarks>
        T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue);

        /// <summary>
        /// Gets a fallback value for a published content property.
        /// </summary>
        /// <param name="content">The published element.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The requested culture.</param>
        /// <param name="segment">The requested segment.</param>
        /// <param name="defaultValue">An optional default value.</param>
        /// <returns>A fallback value, or null.</returns>
        /// <remarks>
        /// <para>This method is called whenever getting the property value for the specified alias, culture and
        /// segment, either returned no property at all, or a property with HasValue(culture, segment) being false.</para>
        /// fixme explain & document priority + merge w/recurse?
        /// </remarks>
        object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority);

        /// <summary>
        /// Gets a fallback value for a published content property.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="content">The published element.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The requested culture.</param>
        /// <param name="segment">The requested segment.</param>
        /// <param name="defaultValue">An optional default value.</param>
        /// <returns>A fallback value, or null.</returns>
        /// <remarks>
        /// <para>This method is called whenever getting the property value for the specified alias, culture and
        /// segment, either returned no property at all, or a property with HasValue(culture, segment) being false.</para>
        /// fixme explain & document priority + merge w/recurse?
        /// </remarks>
        T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority);
    }
}
