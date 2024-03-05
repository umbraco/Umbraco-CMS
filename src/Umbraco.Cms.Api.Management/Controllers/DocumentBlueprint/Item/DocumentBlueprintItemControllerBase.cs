using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DocumentBlueprint}")]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class DocumentBlueprintItemControllerBase : ManagementApiControllerBase
{
}
