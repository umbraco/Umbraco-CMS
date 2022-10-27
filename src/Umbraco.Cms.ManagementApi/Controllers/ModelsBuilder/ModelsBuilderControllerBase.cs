using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilder;

[ApiController]
[VersionedApiBackOfficeRoute("models-builder")]
[OpenApiTag("Models Builder")]
[ApiVersion("1.0")]

public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
