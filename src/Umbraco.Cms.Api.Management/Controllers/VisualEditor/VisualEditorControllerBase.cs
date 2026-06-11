using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.VisualEditor;

/// <summary>
/// Base controller for visual editor management API endpoints.
/// </summary>
[VersionedApiBackOfficeRoute("visual-editor")]
[ApiExplorerSettings(GroupName = "Visual Editor")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public abstract class VisualEditorControllerBase : ManagementApiControllerBase
{
}
