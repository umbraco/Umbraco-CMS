namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides caches (content and media).
    /// </summary>
    /// <remarks>Default implementation for unrelated caches.</remarks>
    class PublishedCaches : IPublishedCaches
    {
        private readonly IPublishedContentCache _contentCache;
        private readonly IPublishedMediaCache _mediaCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedCaches"/> class with a content cache
        /// and a media cache.
        /// </summary>
        public PublishedCaches(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
        {
            _contentCache = contentCache;
            _mediaCache = mediaCache;
        }

        /// <summary>
        /// Creates a contextual content cache for a specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A new contextual content cache for the specified context.</returns>
        public ContextualPublishedContentCache CreateContextualContentCache(UmbracoContext context)
        {
            return new ContextualPublishedContentCache(_contentCache, context);
        }

        /// <summary>
        /// Creates a contextual media cache for a specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A new contextual media cache for the specified context.</returns>
        public ContextualPublishedMediaCache CreateContextualMediaCache(UmbracoContext context)
        {
            return new ContextualPublishedMediaCache(_mediaCache, context);            
        }
    }
}
