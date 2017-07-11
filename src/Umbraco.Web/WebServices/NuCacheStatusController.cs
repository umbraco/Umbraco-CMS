using System;
using System.Web.Http;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.WebServices
{
    public class NuCacheStatusController : UmbracoAuthorizedApiController
    {
        private readonly IFacadeService _facadeService;

        public NuCacheStatusController(IFacadeService facadeService)
        {
            if (facadeService == null) throw new ArgumentNullException(nameof(facadeService));
            _facadeService = facadeService;
        }

        private FacadeService FacadeService
        {
            get
            {
                var svc = _facadeService as FacadeService;
                if (svc == null)
                    throw new NotSupportedException("Not running NuCache.");
                return svc;
            }
        }

        [HttpPost]
        public string RebuildDbCache()
        {
            // fixme - should wrap in a service scope once we have them
            var service = FacadeService;
            service.RebuildContentDbCache();
            service.RebuildMediaDbCache();
            service.RebuildMemberDbCache();
            return service.GetStatus();
        }

        [HttpGet]
        public string GetStatus()
        {
            var service = FacadeService;
            return service.GetStatus();
        }

        [HttpGet]
        public string Collect()
        {
            var service = FacadeService;
            GC.Collect();
            service.Collect();
            return service.GetStatus();
        }

        [HttpPost]
        public void ReloadCache()
        {
            Current.DistributedCache.RefreshAllFacade();
        }
    }
}
