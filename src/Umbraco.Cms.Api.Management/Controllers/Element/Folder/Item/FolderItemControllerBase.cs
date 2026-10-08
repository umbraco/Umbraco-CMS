using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Folder.Item;

/// <summary>
/// Serves as the base controller for element folder item operations within the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Element}/folder")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
public class FolderItemControllerBase : ManagementApiControllerBase;
