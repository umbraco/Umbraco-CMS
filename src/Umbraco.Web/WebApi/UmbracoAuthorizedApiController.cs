using System;
using System.Web;
using System.Web.Http;
using Umbraco.Core.Configuration;
using Umbraco.Web.Security;
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
                    var ctx = TryGetHttpContext();
                    if (ctx.Success == false) 
                        throw new InvalidOperationException("To get a current user, this method must occur in a web request");
                    Security.ValidateCurrentUser(ctx.Result, true);
                    _userisValidated = true;
                }

                return Security.CurrentUser;
            }
        }

    }
}