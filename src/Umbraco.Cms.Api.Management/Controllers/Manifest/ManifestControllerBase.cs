using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Manifest;

[VersionedApiBackOfficeRoute("manifest")]
[ApiExplorerSettings(GroupName = "Manifest")]
public abstract class ManifestControllerBase : ManagementApiControllerBase
{
}
