using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Authorization;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.BackOffice.Controllers
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
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [DisableBrowserCache]
    [UmbracoRequireHttps]
    [CheckIfUserTicketDataIsStale]
    [MiddlewareFilter(typeof(UnhandledExceptionLoggerFilter))]
    public abstract class UmbracoAuthorizedApiController : UmbracoApiController
    {

    }
}
