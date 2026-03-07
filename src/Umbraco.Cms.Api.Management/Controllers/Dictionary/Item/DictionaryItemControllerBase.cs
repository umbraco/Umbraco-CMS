using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;

    /// <summary>
    /// Serves as the base controller for managing dictionary items in the Umbraco CMS Management API.
    /// Provides common functionality and endpoints for derived controllers handling dictionary item operations.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
public class DictionaryItemControllerBase : ManagementApiControllerBase
{
}
