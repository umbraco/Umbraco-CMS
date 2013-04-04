namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides caches (content and media).
    /// </summary>
    /// <remarks>Default implementation for unrelated caches.</remarks>
    class PublishedCaches : IPublishedCaches
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedCaches"/> class with a content cache
        /// and a media cache.
        /// </summary>
        public PublishedCaches(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
        {
            ContentCache = contentCache;
            MediaCache = mediaCache;
        }

        /// <summary>
        /// Gets the content cache.
        /// </summary>
        public IPublishedContentCache ContentCache { get; private set; }

        /// <summary>
        /// Gets the media cache.
        /// </summary>
        public IPublishedMediaCache MediaCache { get; private set; }

        /// <summary>
        /// Creates a contextual content cache for a specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A new contextual content cache for the specified context.</returns>
        public ContextualPublishedContentCache CreateContextualContentCache(UmbracoContext context)
        {
            return new ContextualPublishedContentCache(ContentCache, context);
        }

        /// <summary>
        /// Creates a contextual media cache for a specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A new contextual media cache for the specified context.</returns>
        public ContextualPublishedMediaCache CreateContextualMediaCache(UmbracoContext context)
        {
            return new ContextualPublishedMediaCache(MediaCache, context);            
        }
    }
}
