using System;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Cache
{
    public sealed class LanguageCacheRefresher : PayloadCacheRefresherBase<LanguageCacheRefresher, LanguageCacheRefresher.JsonPayload>

    //CacheRefresherBase<LanguageCacheRefresher>
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

        public override void Refresh(JsonPayload[] payloads)
        {
            if (payloads.Length == 0) return;

            var clearDictionary = false;
            var clearContent = false;

            //clear all no matter what type of payload
            ClearAllIsolatedCacheByEntityType<ILanguage>();

            foreach (var payload in payloads)
            {   
                RefreshDomains(payload.Id);

                switch (payload.ChangeType)
                {
                    case JsonPayload.LanguageChangeType.Update:
                        clearDictionary = true;
                        break;
                    case JsonPayload.LanguageChangeType.Remove:
                        clearDictionary = true;
                        clearContent = true;
                        break;
                    case JsonPayload.LanguageChangeType.ChangeCulture:
                        clearDictionary = true;
                        clearContent = true;
                        break;
                }
            }

            if (clearDictionary)
            {
                ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
            }

            //if this flag is set, we will tell the published snapshot service to refresh ALL content
            if (clearContent)
            {
                var clearContentPayload = new[] { new ContentCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) };
                ContentCacheRefresher.NotifyPublishedSnapshotService(_publishedSnapshotService, AppCaches, clearContentPayload);
            }

            // then trigger event
            base.Refresh(payloads);
        }

        // these events should never trigger
        // everything should be PAYLOAD/JSON

        public override void RefreshAll() => throw new NotSupportedException();

        public override void Refresh(int id) => throw new NotSupportedException();

        public override void Refresh(Guid id) => throw new NotSupportedException();

        public override void Remove(int id) => throw new NotSupportedException();

        #endregion

        private void RefreshDomains(int langId)
        {
            var assignedDomains = _domainService.GetAll(true).Where(x => x.LanguageId.HasValue && x.LanguageId.Value == langId).ToList();

            if (assignedDomains.Count > 0)
            {
                // TODO: this is duplicating the logic in DomainCacheRefresher BUT we cannot inject that into this because it it not registered explicitly in the container,
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

        #region Json

        public class JsonPayload
        {
            public JsonPayload(int id, string isoCode, LanguageChangeType changeType)
            {
                Id = id;
                IsoCode = isoCode;
                ChangeType = changeType;
            }

            public int Id { get; }
            public string IsoCode { get; }
            public LanguageChangeType ChangeType { get; }

            public enum LanguageChangeType
            {
                /// <summary>
                /// A new languages has been added
                /// </summary>
                Add = 0,

                /// <summary>
                /// A language has been deleted
                /// </summary>
                Remove = 1,

                /// <summary>
                /// A language has been updated - but it's culture remains the same
                /// </summary>
                Update = 2,

                /// <summary>
                /// A language has been updated - it's culture has changed
                /// </summary>
                ChangeCulture = 3
            }
        }

        #endregion
    }
}
