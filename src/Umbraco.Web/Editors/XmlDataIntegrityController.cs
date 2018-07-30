using System;
using System.Web.Http;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    [ValidateAngularAntiForgeryToken]
    public class XmlDataIntegrityController : UmbracoAuthorizedApiController
    {
        private readonly PublishedSnapshotService _publishedSnapshotService;

        public XmlDataIntegrityController(IPublishedSnapshotService publishedSnapshotService)
        {
            if (publishedSnapshotService == null) throw new ArgumentNullException(nameof(publishedSnapshotService));
            _publishedSnapshotService = publishedSnapshotService as PublishedSnapshotService;
            if (_publishedSnapshotService == null) throw new NotSupportedException("Unsupported IPublishedSnapshotService, only the Xml one is supported.");
        }

        [HttpPost]
        public bool FixContentXmlTable()
        {
            _publishedSnapshotService.RebuildContentAndPreviewXml();
            return _publishedSnapshotService.VerifyContentAndPreviewXml();
        }

        [HttpPost]
        public bool FixMediaXmlTable()
        {
            _publishedSnapshotService.RebuildMediaXml();
            return _publishedSnapshotService.VerifyMediaXml();
        }

        [HttpPost]
        public bool FixMembersXmlTable()
        {
            _publishedSnapshotService.RebuildMemberXml();
            return _publishedSnapshotService.VerifyMemberXml();
        }

        [HttpGet]
        public bool CheckContentXmlTable()
        {
            return _publishedSnapshotService.VerifyContentAndPreviewXml();
        }

        [HttpGet]
        public bool CheckMediaXmlTable()
        {
            return _publishedSnapshotService.VerifyMediaXml();
        }

        [HttpGet]
        public bool CheckMembersXmlTable()
        {
            return _publishedSnapshotService.VerifyMemberXml();
        }
    }
}
