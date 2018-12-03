using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web.Security;

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

        protected BackOfficeUserManager<BackOfficeIdentityUser> UserManager
            => _userManager ?? (_userManager = TryGetOwinContext().Result.GetBackOfficeUserManager());
    }
}
