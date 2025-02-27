using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Tag;

[VersionedApiBackOfficeRoute("tag")]
[ApiExplorerSettings(GroupName = "Tag")]
public class TagControllerBase : ManagementApiControllerBase
{
}
