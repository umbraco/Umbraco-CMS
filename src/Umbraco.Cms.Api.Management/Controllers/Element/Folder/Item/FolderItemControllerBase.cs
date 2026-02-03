using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Folder.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Element}/folder")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
public class FolderItemControllerBase : ManagementApiControllerBase;
