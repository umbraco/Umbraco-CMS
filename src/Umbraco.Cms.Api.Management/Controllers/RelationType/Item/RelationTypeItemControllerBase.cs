using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.RelationType}")]
[ApiExplorerSettings(GroupName = "Relation Type")]
public class RelationTypeItemControllerBase : ManagementApiControllerBase
{

}
