using System;
using System.Web.Http;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    public class PublishedStatusController : UmbracoAuthorizedApiController
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;

        public PublishedStatusController(IPublishedSnapshotService publishedSnapshotService)
        {
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
        }

        [HttpGet]
        public string GetPublishedStatusUrl()
        {
            if (!string.IsNullOrWhiteSpace(_publishedSnapshotService.StatusUrl))
            {
                return _publishedSnapshotService.StatusUrl;
            }

            throw new NotSupportedException("Not supported: " + _publishedSnapshotService.GetType().FullName);
        }
    }
}
