using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[ApiController]
[VersionedApiBackOfficeRoute("models-builder")]
[ApiExplorerSettings(GroupName = "Models Builder")]
public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
