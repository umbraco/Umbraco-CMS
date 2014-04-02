namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Specifies the acceptable level of cache for a property value.
    /// </summary>
    /// <remarks>By default, <c>Request</c> is assumed.</remarks>
    public enum PropertyCacheLevel
    {
        // note: we use the relative values in PublishedPropertyType to ensure that
        // object level >= source level
        // xpath level >= source level

        /// <summary>
        /// Indicates that the property value can be cached at the content level, ie it can be
        /// cached until the content itself is modified.
        /// </summary>
        Content = 1,

        /// <summary>
        /// Indicates that the property value can be cached at the content cache level, ie it can
        /// be cached until any content in the cache is modified.
        /// </summary>
        ContentCache = 2,

        /// <summary>
        /// Indicates that the property value can be cached at the request level, ie it can be
        /// cached for the duration of the current request.
        /// </summary>
        Request = 3,

        /// <summary>
        /// Indicates that the property value cannot be cached and has to be converted any time
        /// it is requested.
        /// </summary>
        None = 4
    }
}
