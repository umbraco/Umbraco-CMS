using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.VisualEditor;

[VersionedApiBackOfficeRoute("visual-editor")]
[ApiExplorerSettings(GroupName = "Visual Editor")]
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public abstract class VisualEditorControllerBase : ManagementApiControllerBase
{
}
