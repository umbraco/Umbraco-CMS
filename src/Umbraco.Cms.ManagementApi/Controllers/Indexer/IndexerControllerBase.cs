using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Indexer;

[ApiController]
[VersionedApiBackOfficeRoute("indexer")]
[ApiExplorerSettings(GroupName = "Indexer")]
public class IndexerControllerBase : ManagementApiControllerBase
{
}
