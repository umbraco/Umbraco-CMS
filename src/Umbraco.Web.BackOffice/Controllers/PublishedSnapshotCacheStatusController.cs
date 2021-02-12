using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class PublishedSnapshotCacheStatusController : UmbracoAuthorizedApiController
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IPublishedSnapshotStatus _publishedSnapshotStatus;
        private readonly DistributedCache _distributedCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedSnapshotCacheStatusController"/> class.
        /// </summary>
        public PublishedSnapshotCacheStatusController(
            IPublishedSnapshotService publishedSnapshotService,
            IPublishedSnapshotStatus publishedSnapshotStatus,
            DistributedCache distributedCache)
        {
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _publishedSnapshotStatus = publishedSnapshotStatus;
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// Rebuilds the Database cache
        /// </summary>
        [HttpPost]
        public string RebuildDbCache()
        {
            _publishedSnapshotService.Rebuild();
            return _publishedSnapshotStatus.GetStatus();
        }

        /// <summary>
        /// Gets a status report
        /// </summary>
        [HttpGet]
        public string GetStatus() => _publishedSnapshotStatus.GetStatus();

        /// <summary>
        /// Cleans up unused snapshots
        /// </summary>
        [HttpGet]
        public async Task<string> Collect()
        {
            GC.Collect();
            await _publishedSnapshotService.CollectAsync();
            return _publishedSnapshotStatus.GetStatus();
        }

        [HttpPost]
        public void ReloadCache() => _distributedCache.RefreshAllPublishedSnapshot();
    }
}
