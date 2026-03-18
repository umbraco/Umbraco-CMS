using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

/// <summary>
/// Serves as the base controller for API endpoints related to the ModelsBuilder feature in Umbraco.
/// </summary>
[VersionedApiBackOfficeRoute("models-builder")]
[ApiExplorerSettings(GroupName = "Models Builder")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
