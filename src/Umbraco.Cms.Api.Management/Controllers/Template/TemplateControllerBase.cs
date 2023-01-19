using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Template)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Template))]
[ApiVersion("1.0")]
public class TemplateControllerBase : ManagementApiControllerBase
{
}
