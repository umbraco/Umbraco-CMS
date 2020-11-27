using System;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Web.Cache;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]    
    public class PublishedSnapshotCacheStatusController : UmbracoAuthorizedApiController
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly DistributedCache _distributedCache;

        public PublishedSnapshotCacheStatusController(IPublishedSnapshotService publishedSnapshotService, DistributedCache distributedCache)
        {
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _distributedCache = distributedCache;
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
            _distributedCache.RefreshAllPublishedSnapshot();
        }
    }
}
