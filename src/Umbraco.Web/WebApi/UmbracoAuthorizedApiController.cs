using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
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

        protected UmbracoAuthorizedApiController()
        { }

        protected UmbracoAuthorizedApiController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, ILogger logger, IProfilingLogger profilingLogger, IRuntimeState runtimeState)
            : base(globalSettings, umbracoContext, sqlContext, services, applicationCache, logger, profilingLogger, runtimeState)
        { }
    }
}
