using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Examine;

[ApiController]
[VersionedApiBackOfficeRoute("examine")]
[OpenApiTag("Examine")]
public class ExamineControllerBase : ManagementApiControllerBase
{
}
