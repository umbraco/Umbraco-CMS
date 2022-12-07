using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

[ApiController]
[VersionedApiBackOfficeRoute("relation")]
[ApiExplorerSettings(GroupName = "Relation")]
[ApiVersion("1.0")]
// TODO: Implement Authentication
public abstract class RelationControllerBase : ManagementApiControllerBase
{

}
