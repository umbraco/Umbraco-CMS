namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a fallback strategy for getting <see cref="IPublishedElement"/> values.
    /// </summary>
    public interface IPublishedValueFallback
    {
        /// <summary>
        /// Gets a value.
        /// </summary>
        /// <remarks>
        /// <para>This is invoked when getting a value for the specified <paramref name="culture"/> and <paramref name="segment"/>
        /// could not return a value, and fallback rules should apply to get the value for another language and/or segment.</para>
        /// </remarks>
        TValue GetValue<TValue>(IPublishedProperty property, string culture, string segment);
    }

    // fixme question
    // this is working at property level at the moment, should we move it up to element,
    // so that the decision can be made based upon the entire element, other properties, etc?
    // or, would we need the *two* levels?

    /// <summary>
    /// Provides a default implementation of <see cref="IPublishedValueFallback"/> that does not fall back at all.
    /// </summary>
    public class NoPublishedValueFallback : IPublishedValueFallback
    {
        /// <inheritdoc />
        public TValue GetValue<TValue>(IPublishedProperty property, string culture, string segment)
        {
            // we don't implement fallback
            return default;
        }
    }
}
