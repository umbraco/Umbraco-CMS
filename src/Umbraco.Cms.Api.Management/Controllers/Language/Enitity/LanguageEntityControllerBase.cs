using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Enitity;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Items}/{Constants.UdiEntityType.Language}")]
[ApiExplorerSettings(GroupName = "Language")]
public class LanguageEntityControllerBase : ManagementApiControllerBase
{
}
