using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

/// <summary>
/// Serves as the base controller for API endpoints that manage indexer operations in the Umbraco CMS.
/// </summary>
[VersionedApiBackOfficeRoute("indexer")]
[ApiExplorerSettings(GroupName = "Indexer")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class IndexerControllerBase : ManagementApiControllerBase
{
}
