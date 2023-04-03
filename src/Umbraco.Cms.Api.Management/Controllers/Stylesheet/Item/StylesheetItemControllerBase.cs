using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Item;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
public class StylesheetItemControllerBase : ManagementApiControllerBase
{

}
