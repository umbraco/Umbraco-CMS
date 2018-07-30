using System;
using System.Web.Http;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    public class NuCacheStatusController : UmbracoAuthorizedApiController
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;

        public NuCacheStatusController(IPublishedSnapshotService publishedSnapshotService)
        {
            if (publishedSnapshotService == null) throw new ArgumentNullException(nameof(publishedSnapshotService));
            _publishedSnapshotService = publishedSnapshotService;
        }

        private PublishedSnapshotService PublishedSnapshotService
        {
            get
            {
                var svc = _publishedSnapshotService as PublishedSnapshotService;
                if (svc == null)
                    throw new NotSupportedException("Not running NuCache.");
                return svc;
            }
        }

        [HttpPost]
        public string RebuildDbCache()
        {
            // fixme - should wrap in a service scope once we have them
            var service = PublishedSnapshotService;
            service.RebuildContentDbCache();
            service.RebuildMediaDbCache();
            service.RebuildMemberDbCache();
            return service.GetStatus();
        }

        [HttpGet]
        public string GetStatus()
        {
            var service = PublishedSnapshotService;
            return service.GetStatus();
        }

        [HttpGet]
        public string Collect()
        {
            var service = PublishedSnapshotService;
            GC.Collect();
            service.Collect();
            return service.GetStatus();
        }

        [HttpPost]
        public void ReloadCache()
        {
            Current.DistributedCache.RefreshAllPublishedSnapshot();
        }
    }
}
