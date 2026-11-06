using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Item;

/// <summary>
/// Serves as the base controller for handling webhook item-related operations in the API.
/// </summary>
/// <remarks>
/// Unlike most item endpoints, webhook items are not available to every back-office user, as they expose
/// the webhook's target URL and configuration and are not used by pickers outside of the Settings section.
/// </remarks>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Webhook}")]
[ApiExplorerSettings(GroupName = "Webhook")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
public class WebhookItemControllerBase : ManagementApiControllerBase
{
}
