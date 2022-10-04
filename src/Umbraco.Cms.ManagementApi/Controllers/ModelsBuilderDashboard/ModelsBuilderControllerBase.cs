using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

[ApiController]
[VersionedApiBackOfficeRoute("modelsBuilder")]
[OpenApiTag("ModelsBuilder")]
[ApiVersion("1.0")]

public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
