using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Language}")]
[ApiExplorerSettings(GroupName = "Language")]
public class LanguageEntityControllerBase : ManagementApiControllerBase
{
}
