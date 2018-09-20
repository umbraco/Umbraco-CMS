﻿using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Cache
{
    public sealed class MediaCacheRefresher : PayloadCacheRefresherBase<MediaCacheRefresher, MediaCacheRefresher.JsonPayload>
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IdkMap _idkMap;

        public MediaCacheRefresher(CacheHelper cacheHelper, IPublishedSnapshotService publishedSnapshotService, IdkMap idkMap)
            : base(cacheHelper)
        {
            _publishedSnapshotService = publishedSnapshotService;
            _idkMap = idkMap;
        }

        #region Define

        protected override MediaCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("B29286DD-2D40-4DDB-B325-681226589FEC");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Media Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(JsonPayload[] payloads)
        {
            if (payloads == null) return;

            _publishedSnapshotService.Notify(payloads, out var anythingChanged);

            if (anythingChanged)
            {
                Current.ApplicationCache.ClearPartialViewCache();

                var mediaCache = CacheHelper.IsolatedRuntimeCache.GetCache<IMedia>();

                foreach (var payload in payloads)
                {
                    if (payload.ChangeTypes == TreeChangeTypes.Remove)
                       _idkMap.ClearCache(payload.Id);

                    if (!mediaCache) continue;

                    // repository cache
                    // it *was* done for each pathId but really that does not make sense
                    // only need to do it for the current media
                    mediaCache.Result.ClearCacheItem(RepositoryCacheKeys.GetKey<IMedia>(payload.Id));

                    // remove those that are in the branch
                    if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.RefreshBranch | TreeChangeTypes.Remove))
                    {
                        var pathid = "," + payload.Id + ",";
                        mediaCache.Result.ClearCacheObjectTypes<IMedia>((_, v) => v.Path.Contains(pathid));
                    }
                }
            }

            base.Refresh(payloads);
        }

        // these events should never trigger
        // everything should be JSON

        public override void RefreshAll()
        {
            throw new NotSupportedException();
        }

        public override void Refresh(int id)
        {
            throw new NotSupportedException();
        }

        public override void Refresh(Guid id)
        {
            throw new NotSupportedException();
        }

        public override void Remove(int id)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Json

        public class JsonPayload
        {
            public JsonPayload(int id, TreeChangeTypes changeTypes)
            {
                Id = id;
                ChangeTypes = changeTypes;
            }

            public int Id { get; }

            public TreeChangeTypes ChangeTypes { get; }
        }

        #endregion

        #region Indirect

        public static void RefreshMediaTypes(CacheHelper cacheHelper)
        {
            cacheHelper.IsolatedRuntimeCache.ClearCache<IMedia>();
        }

        #endregion
    }
}
