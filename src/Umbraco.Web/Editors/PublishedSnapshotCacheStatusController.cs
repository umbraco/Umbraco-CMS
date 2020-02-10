using System;
using System.Web.Http;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    public class PublishedSnapshotCacheStatusController : UmbracoAuthorizedApiController
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;

        public PublishedSnapshotCacheStatusController(IPublishedSnapshotService publishedSnapshotService)
        {
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
        }

        [HttpPost]
        public string RebuildDbCache()
        {
            _publishedSnapshotService.Rebuild();
            return _publishedSnapshotService.GetStatus();
        }

        [HttpGet]
        public string GetStatus()
        {
            return _publishedSnapshotService.GetStatus();
        }

        [HttpGet]
        public string Collect()
        {
            GC.Collect();
            _publishedSnapshotService.Collect();
            return _publishedSnapshotService.GetStatus();
        }

        [HttpPost]
        public void ReloadCache()
        {
            Current.DistributedCache.RefreshAllPublishedSnapshot();
        }
    }
}
