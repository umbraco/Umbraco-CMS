using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

[ApiController]
[VersionedApiBackOfficeRoute("dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[ApiVersion("1.0")]
// TODO: Add authentication
public abstract class DictionaryControllerBase : ManagementApiControllerBase
{
}
