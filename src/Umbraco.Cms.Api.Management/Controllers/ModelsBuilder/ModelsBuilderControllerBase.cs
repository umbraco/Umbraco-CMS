using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[ApiController]
[VersionedApiBackOfficeRoute("models-builder")]
[ApiExplorerSettings(GroupName = "Models Builder")]
[ApiVersion("1.0")]

public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
