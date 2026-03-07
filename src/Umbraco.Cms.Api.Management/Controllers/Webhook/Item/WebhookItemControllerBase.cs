using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Item;

    /// <summary>
    /// Serves as the base controller for handling webhook item-related operations in the API.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Webhook}")]
[ApiExplorerSettings(GroupName = "Webhook")]
public class WebhookItemControllerBase : ManagementApiControllerBase
{
}
