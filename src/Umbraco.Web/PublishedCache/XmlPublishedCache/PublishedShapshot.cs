using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    /// <summary>
    /// Implements a published snapshot.
    /// </summary>
    class PublishedShapshot : IPublishedShapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedShapshot"/> class with a content cache
        /// and a media cache.
        /// </summary>
        public PublishedShapshot(
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

        /// <inheritdoc />
        public IPublishedContentCache ContentCache { get; }

        /// <inheritdoc />
        public IPublishedMediaCache MediaCache { get; }

        /// <inheritdoc />
        public IPublishedMemberCache MemberCache { get; }

        /// <inheritdoc />
        public IDomainCache DomainCache { get; }

        /// <inheritdoc />
        public ICacheProvider SnapshotCache => null;

        /// <inheritdoc />
        public ICacheProvider ElementsCache => null;

        /// <inheritdoc />
        public IDisposable ForcedPreview(bool preview, Action<bool> callback = null)
        {
            // the XML cache does not support forcing preview, really, so, just pretend...
            return new ForcedPreviewObject();
        }

        private class ForcedPreviewObject : DisposableObject
        {
            protected override void DisposeResources()
            { }
        }
    }
}
