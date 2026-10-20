using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

/// <summary>
/// Serves as the base controller for API endpoints related to searcher management operations in Umbraco.
/// </summary>
[VersionedApiBackOfficeRoute("searcher")]
[ApiExplorerSettings(GroupName = "Searcher")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class SearcherControllerBase : ManagementApiControllerBase
{
}
