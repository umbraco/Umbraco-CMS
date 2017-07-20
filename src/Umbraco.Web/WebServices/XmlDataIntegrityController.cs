using System;
using System.Web.Http;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebServices
{
    [ValidateAngularAntiForgeryToken]
    public class XmlDataIntegrityController : UmbracoAuthorizedApiController
    {
        private readonly FacadeService _facadeService;

        public XmlDataIntegrityController(IFacadeService facadeService)
        {
            if (facadeService == null) throw new ArgumentNullException(nameof(facadeService));
            _facadeService = facadeService as FacadeService;
            if (_facadeService == null) throw new NotSupportedException("Unsupported IFacadeService, only the Xml one is supported.");
        }

        [HttpPost]
        public bool FixContentXmlTable()
        {
            _facadeService.RebuildContentAndPreviewXml();
            return _facadeService.VerifyContentAndPreviewXml();
        }

        [HttpPost]
        public bool FixMediaXmlTable()
        {
            _facadeService.RebuildMediaXml();
            return _facadeService.VerifyMediaXml();
        }

        [HttpPost]
        public bool FixMembersXmlTable()
        {
            _facadeService.RebuildMemberXml();
            return _facadeService.VerifyMemberXml();
        }

        [HttpGet]
        public bool CheckContentXmlTable()
        {
            return _facadeService.VerifyContentAndPreviewXml();
        }

        [HttpGet]
        public bool CheckMediaXmlTable()
        {
            return _facadeService.VerifyMediaXml();
        }

        [HttpGet]
        public bool CheckMembersXmlTable()
        {
            return _facadeService.VerifyMemberXml();
        }
    }
}
