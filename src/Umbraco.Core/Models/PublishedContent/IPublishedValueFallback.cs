namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a fallback strategy for getting <see cref="IPublishedElement"/> values.
    /// </summary>
    public interface IPublishedValueFallback
    {
        // fixme discussions & challenges
        //
        // - what's with visitedLanguage? should be internal to fallback implementation
        //    so that should be the case now, with latest changes
        //
        // - should be as simple as
        //     model.Value("price", fallback: ValueFallback.Language);
        //     model.Value("name", fallback: ValueFallback.Recurse);
        //
        //   so chaining things through an array of ints is not... convenient
        //   it feels like ppl could have ValueFallback.LanguageAndRecurse or something?
        //
        // - the fallback: parameter value must be open, so about anything can be passed to the IPublishedValueFallback
        //    we have it now, it's an integer + constants, cool
        //
        // - we need to be able to configure (via code for now) a default fallback policy?
        //    not! the default value of the fallback: parameter is 'default', not 'none', and if people
        //    want to implement a different default behavior, they have to override the fallback provider
        //
        // - currently, no policies on IPublishedProperty nor IPublishedElement, but some may apply (language)
        //    todo: implement
        //
        // - general defaultValue discussion:
        //     when HasValue is false, the converter may return something, eg an empty enumerable, even though
        //     defaultValue is null, so should we respect defaultValue only when it is not 'default'?
        //     todo: when defaultValue==default, and HasValue is false, still return GetValue to ensure this
        //
        // - (and...)
        //    ModelsBuilder model.Value(x => x.Price, ...) extensions need to be adjusted too
        //
        // - cache & perfs
        //     soon as ppl implement custom fallbacks, caching is a problem, so better just not cache
        //     OTOH we need to implement the readonly thing for languages

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
        object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, int fallback);

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
        T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, int fallback);
    }
}
