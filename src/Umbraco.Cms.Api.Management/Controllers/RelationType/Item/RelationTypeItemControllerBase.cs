using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.RelationType}")]
[ApiExplorerSettings(GroupName = "Relation Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessRelationTypes)]
public class RelationTypeItemControllerBase : ManagementApiControllerBase
{
}
