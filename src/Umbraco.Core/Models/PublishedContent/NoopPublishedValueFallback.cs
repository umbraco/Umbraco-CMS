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
        public object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue, int fallback) => defaultValue;

        /// <inheritdoc />
        public T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue, int fallback) => defaultValue;

        /// <inheritdoc />
        public object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue, int fallback) => defaultValue;

        /// <inheritdoc />
        public T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue, int fallback) => defaultValue;

        /// <inheritdoc />
        public object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, int fallback) => defaultValue;

        /// <inheritdoc />
        public T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, int fallback) => defaultValue;
    }
}
