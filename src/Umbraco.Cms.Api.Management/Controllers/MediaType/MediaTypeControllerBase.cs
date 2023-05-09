using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.MediaType)]
[ApiExplorerSettings(GroupName = "Media Type")]
public abstract class MediaTypeControllerBase : ManagementApiControllerBase
{
}
