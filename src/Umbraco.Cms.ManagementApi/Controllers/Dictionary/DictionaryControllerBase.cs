using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/dictionary")]
[OpenApiTag("Dictionary")]
[ApiVersion("1.0")]
// TODO: Add authentication
public abstract class DictionaryControllerBase : ManagementApiControllerBase
{
}
