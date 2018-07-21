using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a noop implementation for <see cref="IPublishedValueFallback"/>.
    /// </summary>
    /// <remarks>
    /// <para>This is for tests etc - does not implement fallback at all.</para>
    /// </remarks>
    public class NoopPublishedValueFallback : IPublishedValueFallback
    {
        /// <inheritdoc />
        public object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue, ICollection<int> visitedLanguages) => defaultValue;

        /// <inheritdoc />
        public T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages) => defaultValue;

        /// <inheritdoc />
        public object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue, ICollection<int> visitedLanguages) => defaultValue;

        /// <inheritdoc />
        public T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages) => defaultValue;

        /// <inheritdoc />
        public object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, IEnumerable<int> fallbackMethods, ICollection<int> visitedLanguages) => defaultValue;

        /// <inheritdoc />
        public T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, IEnumerable<int> fallbackMethods, ICollection<int> visitedLanguages) => defaultValue;
    }
}
