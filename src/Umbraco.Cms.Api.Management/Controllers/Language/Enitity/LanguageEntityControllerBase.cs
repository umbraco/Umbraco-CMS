using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Enitity;

[ApiController]
[VersionedApiBackOfficeRoute($"/{Constants.UdiEntityType.Language}/{Constants.Web.RoutePath.Items}")]
[ApiExplorerSettings(GroupName = "Language")]
public class LanguageEntityControllerBase : ManagementApiControllerBase
{
}
