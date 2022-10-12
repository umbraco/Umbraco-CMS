using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Install;

[ApiController]
[VersionedApiBackOfficeRoute("install")]
[OpenApiTag("Install")]
[RequireRuntimeLevel(RuntimeLevel.Install)]
public abstract class InstallControllerBase : ManagementApiControllerBase
{
}
