using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

/// <summary>
/// Serves as the base controller for API endpoints that manage the published cache in Umbraco CMS.
/// </summary>
[VersionedApiBackOfficeRoute("published-cache")]
[ApiExplorerSettings(GroupName = "Published Cache")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
