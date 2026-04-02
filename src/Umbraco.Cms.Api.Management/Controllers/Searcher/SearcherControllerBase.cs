using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

/// <summary>
/// Serves as the base controller for API endpoints related to searcher management operations in Umbraco.
/// </summary>
[VersionedApiBackOfficeRoute("searcher")]
[ApiExplorerSettings(GroupName = "Searcher")]
public class SearcherControllerBase : ManagementApiControllerBase
{
}
