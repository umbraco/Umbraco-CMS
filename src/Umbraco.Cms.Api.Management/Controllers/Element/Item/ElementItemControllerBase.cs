using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Item;

/// <summary>
/// Serves as the base controller for element item operations within the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Element}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
public class ElementItemControllerBase : ManagementApiControllerBase;
