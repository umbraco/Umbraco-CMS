using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Culture;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/culture")]
[OpenApiTag("Culture")]
public abstract class CultureControllerBase : Controller
{
}
