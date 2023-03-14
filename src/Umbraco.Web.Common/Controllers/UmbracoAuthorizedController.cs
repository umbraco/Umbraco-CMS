using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Filters;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Provides a base class for backoffice authorized controllers.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[DisableBrowserCache]
public abstract class UmbracoAuthorizedController : ControllerBase
{
}
