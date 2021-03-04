﻿using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;

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
    // [DisableBrowserCache] has been migrated to netcore
    // [UmbracoWebApiRequireHttps]
    // [CheckIfUserTicketDataIsStale]
    // [UnhandedExceptionLoggerConfiguration]
    [EnableDetailedErrors]
    public abstract class UmbracoAuthorizedApiController : UmbracoApiController
    {
        protected UmbracoAuthorizedApiController()
        {
        }

        protected UmbracoAuthorizedApiController(GlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
            : base(globalSettings, umbracoContextAccessor, backOfficeSecurityAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider)
        {
        }

    }
}
