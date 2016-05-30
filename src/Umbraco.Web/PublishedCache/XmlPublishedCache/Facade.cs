namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    /// <summary>
    /// Implements a facade.
    /// </summary>
    class Facade : IFacade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Facade"/> class with a content cache
        /// and a media cache.
        /// </summary>
        public Facade(
            PublishedContentCache contentCache,
            PublishedMediaCache mediaCache,
            PublishedMemberCache memberCache,
            DomainCache domainCache)
        {
            ContentCache = contentCache;
            MediaCache = mediaCache;
            MemberCache = memberCache;
            DomainCache = domainCache;
        }

        /// <summary>
        /// Gets the <see cref="IPublishedContentCache"/>.
        /// </summary>
        public IPublishedContentCache ContentCache { get; }

        /// <summary>
        /// Gets the <see cref="IPublishedMediaCache"/>.
        /// </summary>
        public IPublishedMediaCache MediaCache { get; }

        /// <summary>
        /// Gets the <see cref="IPublishedMemberCache"/>.
        /// </summary>
        public IPublishedMemberCache MemberCache { get; }

        /// <summary>
        /// Gets the <see cref="IDomainCache"/>.
        /// </summary>
        public IDomainCache DomainCache { get; }
    }
}
