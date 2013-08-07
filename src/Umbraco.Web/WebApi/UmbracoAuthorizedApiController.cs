using System;
using System.Web;
using System.Web.Http;
using Umbraco.Core.Configuration;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;
using umbraco.BusinessLogic;

namespace Umbraco.Web.WebApi
{
    [UmbracoAuthorize]
    public abstract class UmbracoAuthorizedApiController : UmbracoApiController
    {
        protected UmbracoAuthorizedApiController()
        {
            
        }

        protected UmbracoAuthorizedApiController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }
        
        private bool _userisValidated = false;
        
        /// <summary>
        /// Returns the currently logged in Umbraco User
        /// </summary>
        protected User UmbracoUser
        {
            get
            {                
                //throw exceptions if not valid (true)
                if (!_userisValidated)
                {
                    Security.ValidateCurrentUser(true);
                    _userisValidated = true;
                }

                return Security.CurrentUser;
            }
        }

    }
}