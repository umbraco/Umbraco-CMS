using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Tag;

[ApiController]
[VersionedApiBackOfficeRoute("tag")]
[ApiExplorerSettings(GroupName = "Tag")]
[ApiVersion("1.0")]
public class TagControllerBase : ManagementApiControllerBase
{
}
