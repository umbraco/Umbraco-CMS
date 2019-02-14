using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    /// <summary>
    /// Implements a published snapshot.
    /// </summary>
    class PublishedSnapshot : IPublishedSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedSnapshot"/> class with a content cache
        /// and a media cache.
        /// </summary>
        public PublishedSnapshot(
            PublishedContentCache contentCache,
            PublishedMediaCache mediaCache,
            PublishedMemberCache memberCache,
            DomainCache domainCache)
        {
            Content = contentCache;
            Media = mediaCache;
            Members = memberCache;
            Domains = domainCache;
        }

        /// <inheritdoc />
        public IPublishedContentCache Content { get; }

        /// <inheritdoc />
        public IPublishedMediaCache Media { get; }

        /// <inheritdoc />
        public IPublishedMemberCache Members { get; }

        /// <inheritdoc />
        public IDomainCache Domains { get; }

        /// <inheritdoc />
        public IAppCache SnapshotCache => null;

        /// <inheritdoc />
        public IAppCache ElementsCache => null;

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

        public void Dispose()
        { }
    }
}
