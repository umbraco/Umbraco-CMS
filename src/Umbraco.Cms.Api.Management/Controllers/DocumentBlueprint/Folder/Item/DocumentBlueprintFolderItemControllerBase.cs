using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder.Item;

/// <summary>
/// Serves as the base controller for document blueprint folder item operations within the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DocumentBlueprint}/folder")]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
public class DocumentBlueprintFolderItemControllerBase : ManagementApiControllerBase;
