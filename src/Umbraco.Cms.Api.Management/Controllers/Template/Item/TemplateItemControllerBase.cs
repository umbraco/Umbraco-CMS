using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Item;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Template}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Template))]
public class TemplateItemControllerBase : ManagementApiControllerBase
{

}
