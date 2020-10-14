using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using Umbraco.Core.Mapping;
using Umbraco.Web.Routing;

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
    // [UmbracoUserTimeoutFilter] has been migrated to netcore
    [UmbracoAuthorize]
    [DisableBrowserCache]
   // [UmbracoWebApiRequireHttps]
   // [CheckIfUserTicketDataIsStale]
    // [UnhandedExceptionLoggerConfiguration]
    [EnableDetailedErrors]
    public abstract class UmbracoAuthorizedApiController : UmbracoApiController
    {
        private BackOfficeOwinUserManager _userManager;

        protected UmbracoAuthorizedApiController()
        {
        }

        protected UmbracoAuthorizedApiController(GlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider)
        {
        }

        /// <summary>
        /// Gets the user manager.
        /// </summary>
        protected BackOfficeOwinUserManager UserManager
            => _userManager ?? (_userManager = TryGetOwinContext().Result.GetBackOfficeUserManager());
    }
}
