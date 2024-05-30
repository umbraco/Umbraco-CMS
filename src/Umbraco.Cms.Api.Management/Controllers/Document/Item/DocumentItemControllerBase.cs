using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
public class DocumentItemControllerBase : ManagementApiControllerBase
{
}
