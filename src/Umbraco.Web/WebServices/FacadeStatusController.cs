using System;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.WebServices
{
    public class FacadeStatusController : UmbracoAuthorizedApiController
    {
        private readonly IFacadeService _facadeService;

        public FacadeStatusController(IFacadeService facadeService)
        {
            if (facadeService == null) throw new ArgumentNullException(nameof(facadeService));
            _facadeService = facadeService;
        }

        [HttpGet]
        public string GetFacadeStatusUrl()
        {
            if (_facadeService is Umbraco.Web.PublishedCache.XmlPublishedCache.FacadeService)
                return "views/dashboard/developer/xmldataintegrityreport.html";
            //if (service is PublishedCache.PublishedNoCache.FacadeService)
            //    return "views/dashboard/developer/nocache.html";
            if (_facadeService is PublishedCache.NuCache.FacadeService)
                return "views/dashboard/developer/nucache.html";
            throw new NotSupportedException("Not supported: " + _facadeService.GetType().FullName);
        }
    }
}
