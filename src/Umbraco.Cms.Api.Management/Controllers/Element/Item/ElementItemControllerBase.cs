using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Element}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
public class ElementItemControllerBase : ManagementApiControllerBase;
