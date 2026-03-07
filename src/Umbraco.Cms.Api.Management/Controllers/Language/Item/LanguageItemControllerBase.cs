using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

    /// <summary>
    /// Serves as the base controller for operations related to language items in the Umbraco CMS Management API.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Language}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Language))]
public class LanguageItemControllerBase : ManagementApiControllerBase
{
}
