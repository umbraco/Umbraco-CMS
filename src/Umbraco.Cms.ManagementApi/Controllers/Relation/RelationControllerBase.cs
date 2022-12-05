using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

[ApiController]
[VersionedApiBackOfficeRoute("relation")]
[ApiExplorerSettings(GroupName = "Relation")]
[ApiVersion("1.0")]
// TODO: Implement Authentication
public abstract class RelationControllerBase : ManagementApiControllerBase
{

}
