using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Culture;

[ApiController]
[VersionedApiBackOfficeRoute("culture")]
[OpenApiTag("Culture")]
[ApiVersion("1.0")]
public abstract class CultureControllerBase : ManagementApiControllerBase
{
}
