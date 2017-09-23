using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Provides a base class for autorized auto-routed Umbraco API controllers.
    /// </summary>
    /// <remarks>
    /// This controller will also append a custom header to the response if the user is logged in using forms authentication
    /// which indicates the seconds remaining before their timeout expires.
    /// </remarks>
    [IsBackOffice]
    [UmbracoUserTimeoutFilter]
    [UmbracoAuthorize]
    [DisableBrowserCache]
    [UmbracoWebApiRequireHttps]
    [CheckIfUserTicketDataIsStale]
    [UnhandedExceptionLoggerConfiguration]
    [EnableDetailedErrors]
    public abstract class UmbracoAuthorizedApiController : UmbracoApiController
    {
        private BackOfficeUserManager<BackOfficeIdentityUser> _userManager;
        private bool _userisValidated = false;

        protected BackOfficeUserManager<BackOfficeIdentityUser> UserManager
            => _userManager ?? (_userManager = TryGetOwinContext().Result.GetBackOfficeUserManager());

        /// <summary>
        /// Returns the currently logged in Umbraco User
        /// </summary>
        /*
        [Obsolete("This should no longer be used since it returns the legacy user object, use The Security.CurrentUser instead to return the proper user object, or Security.GetUserId() if you want to just get the user id")]
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

                return new User(Security.CurrentUser);
            }
        }
        */ // fixme v8 remove this code
    }
}
