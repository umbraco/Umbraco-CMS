using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

    /// <summary>
    /// Serves as the base controller for managing media items via the Umbraco CMS Management API.
    /// Provides common functionality for derived media item controllers.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
public class MediaItemControllerBase : ManagementApiControllerBase
{
}
