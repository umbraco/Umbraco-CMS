using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[VersionedApiBackOfficeRoute("models-builder")]
[ApiExplorerSettings(GroupName = "Models Builder")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
