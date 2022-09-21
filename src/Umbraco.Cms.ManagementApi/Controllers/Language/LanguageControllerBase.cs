using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/language")]
[OpenApiTag("Language")]
[ApiVersion("1.0")]
public abstract class LanguageControllerBase : ManagementApiControllerBase
{
}
