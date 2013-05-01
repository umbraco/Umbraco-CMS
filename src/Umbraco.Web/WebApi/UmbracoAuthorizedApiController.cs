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

        private User _user;
        private bool _userisValidated = false;

        /// <summary>
        /// The current user ID
        /// </summary>
        private int _uid = 0;

        /// <summary>
        /// The page timeout in seconds.
        /// </summary>
        private long _timeout = 0;

        /// <summary>
        /// Returns the currently logged in Umbraco User
        /// </summary>
        protected User UmbracoUser
        {
            get
            {
                if (!_userisValidated) ValidateUser();
                return _user;
            }
        }

        private void ValidateUser()
        {
            if ((UmbracoContext.Security.UmbracoUserContextId != ""))
            {
                _uid = UmbracoContext.Security.GetUserId(UmbracoContext.Security.UmbracoUserContextId);
                _timeout = UmbracoContext.Security.GetTimeout(UmbracoContext.Security.UmbracoUserContextId);

                if (_timeout > DateTime.Now.Ticks)
                {
                    _user = global::umbraco.BusinessLogic.User.GetUser(_uid);

                    // Check for console access
                    if (_user.Disabled || (_user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(HttpContext.Current) && !GlobalSettings.RequestIsLiveEditRedirector(HttpContext.Current)))
                    {
                        throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                    }
                    _userisValidated = true;
                    UmbracoContext.Security.UpdateLogin(_timeout);
                }
                else
                {
                    throw new ArgumentException("User has timed out!!");
                }
            }
            else
            {
                throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
            }

        }
    }
}