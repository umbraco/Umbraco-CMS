using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilder;

[ApiController]
[VersionedApiBackOfficeRoute("models-builder")]
[ApiExplorerSettings(GroupName = "Models Builder")]
[ApiVersion("1.0")]

public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
