using System;
using System.Web;
using System.Web.Http;
using Umbraco.Core.Configuration;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;
using umbraco.BusinessLogic;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// A base controller that ensures all requests are authorized - the user is logged in. 
    /// </summary>
    /// <remarks>
    /// This controller will also append a custom header to the response if the user is logged in using forms authentication 
    /// which indicates the seconds remaining before their timeout expires.
    /// </remarks>
    [IsBackOffice]
    [UmbracoUserTimeoutFilter]
    [UmbracoAuthorize]
    [DisableBrowserCache]
    public abstract class UmbracoAuthorizedApiController : UmbracoApiController
    {
        protected UmbracoAuthorizedApiController()
        {
        }

        protected UmbracoAuthorizedApiController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }

        protected UmbracoAuthorizedApiController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper) : base(umbracoContext, umbracoHelper)
        {
        }
        
    }
}