using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Search;

/// <summary>
/// Serves as the base controller for search and index management endpoints in the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute("search")]
[ApiExplorerSettings(GroupName = "Search")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public abstract class SearchControllerBase : ManagementApiControllerBase
{
}
