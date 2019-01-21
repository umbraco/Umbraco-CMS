using System;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Cache
{
    public sealed class LanguageCacheRefresher : CacheRefresherBase<LanguageCacheRefresher>
    {
        public LanguageCacheRefresher(AppCaches appCaches, IPublishedSnapshotService publishedSnapshotService, IDomainService domainService)
            : base(appCaches)
        {
            _publishedSnapshotService = publishedSnapshotService;
            _domainService = domainService;
        }

        #region Define

        protected override LanguageCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("3E0F95D8-0BE5-44B8-8394-2B8750B62654");
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IDomainService _domainService;

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Language Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(int id)
        {
            ClearAllIsolatedCacheByEntityType<ILanguage>();
            RefreshDomains(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearAllIsolatedCacheByEntityType<ILanguage>();
            //if a language is removed, then all dictionary cache needs to be removed
            ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
            RefreshDomains(id);
            base.Remove(id);
        }

        #endregion

        private void RefreshDomains(int langId)
        {
            var assignedDomains = _domainService.GetAll(true).Where(x => x.LanguageId.HasValue && x.LanguageId.Value == langId).ToList();

            if (assignedDomains.Count > 0)
            {
                //fixme - this is duplicating the logic in DomainCacheRefresher BUT we cannot inject that into this because it it not registered explicitly in the container,
                // and we cannot inject the CacheRefresherCollection since that would be a circular reference, so what is the best way to call directly in to the
                // DomainCacheRefresher?

                ClearAllIsolatedCacheByEntityType<IDomain>();
                // note: must do what's above FIRST else the repositories still have the old cached
                // content and when the PublishedCachesService is notified of changes it does not see
                // the new content...
                // notify
                _publishedSnapshotService.Notify(assignedDomains.Select(x => new DomainCacheRefresher.JsonPayload(x.Id, DomainChangeTypes.Remove)).ToArray());
            }
        }
    }
}
