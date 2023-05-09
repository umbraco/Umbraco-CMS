using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
public class StylesheetItemControllerBase : ManagementApiControllerBase
{

}
