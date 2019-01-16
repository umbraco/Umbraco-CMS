using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Provides a base class for authorized auto-routed Umbraco API controllers.
    /// </summary>
    /// <remarks>
    /// This controller will also append a custom header to the response if the user
    /// is logged in using forms authentication which indicates the seconds remaining
    /// before their timeout expires.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoAuthorizedApiController"/> with auto dependencies.
        /// </summary>
        /// <remarks>Dependencies are obtained from the <see cref="Current"/> service locator.</remarks>
        protected UmbracoAuthorizedApiController()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoAuthorizedApiController"/> class with all its dependencies.
        /// </summary>
        protected UmbracoAuthorizedApiController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, IProfilingLogger logger, IRuntimeState runtimeState)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, applicationCache, logger, runtimeState)
        { }

        /// <summary>
        /// Gets the user manager.
        /// </summary>
        protected BackOfficeUserManager<BackOfficeIdentityUser> UserManager
            => _userManager ?? (_userManager = TryGetOwinContext().Result.GetBackOfficeUserManager());
    }
}
