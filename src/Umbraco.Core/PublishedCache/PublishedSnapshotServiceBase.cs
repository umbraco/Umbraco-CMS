using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Cache;

namespace Umbraco.Web.PublishedCache
{
    // TODO: This base class probably shouldn't exist
    public abstract class PublishedSnapshotServiceBase : IPublishedSnapshotService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedSnapshotServiceBase"/> class.
        /// </summary>
        protected PublishedSnapshotServiceBase(IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            PublishedSnapshotAccessor = publishedSnapshotAccessor;
            VariationContextAccessor = variationContextAccessor;
        }

        /// <inheritdoc/>
        public IPublishedSnapshotAccessor PublishedSnapshotAccessor { get; }

        /// <summary>
        /// Gets the <see cref="IVariationContextAccessor"/>
        /// </summary>
        public IVariationContextAccessor VariationContextAccessor { get; }

        // note: NOT setting _publishedSnapshotAccessor.PublishedSnapshot here because it is the
        // responsibility of the caller to manage what the 'current' facade is

        /// <inheritdoc/>
        public abstract IPublishedSnapshot CreatePublishedSnapshot(string previewToken);

        protected IPublishedSnapshot CurrentPublishedSnapshot => PublishedSnapshotAccessor.PublishedSnapshot;

        /// <inheritdoc/>
        public abstract bool EnsureEnvironment(out IEnumerable<string> errors);

        /// <inheritdoc/>
        public abstract void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged);

        /// <inheritdoc/>
        public abstract void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged);

        /// <inheritdoc/>
        public abstract void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads);

        /// <inheritdoc/>
        public abstract void Notify(DataTypeCacheRefresher.JsonPayload[] payloads);

        /// <inheritdoc/>
        public abstract void Notify(DomainCacheRefresher.JsonPayload[] payloads);

        /// <inheritdoc/>
        public abstract void Rebuild(
            int groupSize = 5000,
            IReadOnlyCollection<int> contentTypeIds = null,
            IReadOnlyCollection<int> mediaTypeIds = null,
            IReadOnlyCollection<int> memberTypeIds = null);

        /// <inheritdoc/>
        public virtual void Dispose()
        { }

        /// <inheritdoc/>
        public abstract string GetStatus();

        /// <inheritdoc/>
        public virtual string StatusUrl => "views/dashboard/settings/publishedsnapshotcache.html";

        /// <inheritdoc/>
        public virtual void Collect()
        {
        }

    }
}
