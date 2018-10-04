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
            if (_publishedSnapshotService is PublishedCache.XmlPublishedCache.PublishedSnapshotService)
                return "views/dashboard/settings/xmldataintegrityreport.html";

            //if (service is PublishedCache.PublishedNoCache.PublishedSnapshotService)
            //    return "views/dashboard/developer/nocache.html";

            if (_publishedSnapshotService is PublishedCache.NuCache.PublishedSnapshotService)
                return "views/dashboard/settings/nucache.html";

            throw new NotSupportedException("Not supported: " + _publishedSnapshotService.GetType().FullName);
        }
    }
}
