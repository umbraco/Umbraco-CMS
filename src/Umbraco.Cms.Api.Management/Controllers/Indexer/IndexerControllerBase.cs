using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

/// <summary>
/// Serves as the base controller for API endpoints that manage indexer operations in the Umbraco CMS.
/// </summary>
[VersionedApiBackOfficeRoute("indexer")]
[ApiExplorerSettings(GroupName = "Indexer")]
public class IndexerControllerBase : ManagementApiControllerBase
{
}
