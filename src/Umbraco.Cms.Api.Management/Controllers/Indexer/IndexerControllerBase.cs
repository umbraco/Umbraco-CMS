using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

[VersionedApiBackOfficeRoute("indexer")]
[ApiExplorerSettings(GroupName = "Indexer")]
public class IndexerControllerBase : ManagementApiControllerBase
{
}
