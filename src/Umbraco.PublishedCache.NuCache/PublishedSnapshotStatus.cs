using Umbraco.Infrastructure.PublishedCache.Persistence;

namespace Umbraco.Web.PublishedCache.NuCache
{
    /// <summary>
    /// Generates a status report for <see cref="PublishedSnapshotService"/>
    /// </summary>
    internal class PublishedSnapshotStatus : IPublishedSnapshotStatus
    {
        private readonly PublishedSnapshotService _service;
        private readonly INuCacheContentService _publishedContentService;

        public PublishedSnapshotStatus(PublishedSnapshotService service, INuCacheContentService publishedContentService)
        {
            _service = service;
            _publishedContentService = publishedContentService;
        }

        /// <inheritdoc/>
        public virtual string StatusUrl => "views/dashboard/settings/publishedsnapshotcache.html";

        /// <inheritdoc/>
        public string GetStatus()
        {
            _service.EnsureCaches();

            var dbCacheIsOk = _publishedContentService.VerifyContentDbCache()
                && _publishedContentService.VerifyMediaDbCache()
                && _publishedContentService.VerifyMemberDbCache();

            ContentStore contentStore = _service.GetContentStore();
            ContentStore mediaStore = _service.GetMediaStore();

            var cg = contentStore.GenCount;
            var mg = mediaStore.GenCount;
            var cs = contentStore.SnapCount;
            var ms = mediaStore.SnapCount;
            var ce = contentStore.Count;
            var me = mediaStore.Count;

            return
                " Database cache is " + (dbCacheIsOk ? "ok" : "NOT ok (rebuild?)") + "." +
                " ContentStore contains " + ce + " item" + (ce > 1 ? "s" : "") +
                " and has " + cg + " generation" + (cg > 1 ? "s" : "") +
                " and " + cs + " snapshot" + (cs > 1 ? "s" : "") + "." +
                " MediaStore contains " + me + " item" + (ce > 1 ? "s" : "") +
                " and has " + mg + " generation" + (mg > 1 ? "s" : "") +
                " and " + ms + " snapshot" + (ms > 1 ? "s" : "") + ".";
        }
    }
}
