using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

[ApiController]
[VersionedApiBackOfficeRoute("language")]
[ApiExplorerSettings(GroupName = "Language")]
[ApiVersion("1.0")]
public abstract class LanguageControllerBase : ManagementApiControllerBase
{
}
