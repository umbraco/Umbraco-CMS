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
    }
}
