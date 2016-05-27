using System;
using System.Web.Http;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.WebServices
{
    public class FacadeStatusController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public string GetFacadeStatusUrl()
        {
            var service = FacadeServiceResolver.Current.Service;
            if (service is Umbraco.Web.PublishedCache.XmlPublishedCache.FacadeService)
                return "views/dashboard/developer/xmldataintegrityreport.html";
            //if (service is PublishedCache.PublishedNoCache.FacadeService)
            //    return "views/dashboard/developer/nocache.html";
            if (service is PublishedCache.NuCache.FacadeService)
                return "views/dashboard/developer/nucache.html";
            throw new NotSupportedException("Not supported: " + service.GetType().FullName);
        }
    }
}
