using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

[ApiController]
[VersionedApiBackOfficeRoute("relationType")]
[ApiExplorerSettings(GroupName = "RelationType")]
[ApiVersion("1.0")]
public class RelationTypeControllerBase : ManagementApiControllerBase
{
}
