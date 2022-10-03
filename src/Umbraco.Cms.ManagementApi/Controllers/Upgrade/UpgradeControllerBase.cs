using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Upgrade;

// TODO: This needs to be an authorized controller.

[ApiController]
[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
[OpenApiTag("Upgrade")]
public abstract class UpgradeControllerBase : ManagementApiControllerBase
{

}
