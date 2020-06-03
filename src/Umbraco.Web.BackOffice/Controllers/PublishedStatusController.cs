using System;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.BackOffice.Controllers
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
