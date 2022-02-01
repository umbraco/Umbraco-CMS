using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Cache;

namespace Umbraco.Web.PublishedCache
{
    internal abstract class PublishedSnapshotServiceBase : IPublishedSnapshotService
    {
        protected PublishedSnapshotServiceBase(IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            PublishedSnapshotAccessor = publishedSnapshotAccessor;
            VariationContextAccessor = variationContextAccessor;
        }

        public IPublishedSnapshotAccessor PublishedSnapshotAccessor { get; }
        public IVariationContextAccessor VariationContextAccessor { get; }

        // note: NOT setting _publishedSnapshotAccessor.PublishedSnapshot here because it is the
        // responsibility of the caller to manage what the 'current' facade is
        public abstract IPublishedSnapshot CreatePublishedSnapshot(string previewToken);

        protected IPublishedSnapshot CurrentPublishedSnapshot => PublishedSnapshotAccessor.PublishedSnapshot;

        public abstract bool EnsureEnvironment(out IEnumerable<string> errors);

        public abstract string EnterPreview(IUser user, int contentId);
        public abstract void RefreshPreview(string previewToken, int contentId);
        public abstract void ExitPreview(string previewToken);
        public abstract void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged);
        public abstract void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged);
        public abstract void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads);
        public abstract void Notify(DataTypeCacheRefresher.JsonPayload[] payloads);
        public abstract void Notify(DomainCacheRefresher.JsonPayload[] payloads);

        public virtual void Rebuild()
        { }

        public virtual void Dispose()
        { }
    }
}
