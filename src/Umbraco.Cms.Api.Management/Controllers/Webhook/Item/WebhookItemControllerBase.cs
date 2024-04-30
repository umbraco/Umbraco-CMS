using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Webhook}")]
[ApiExplorerSettings(GroupName = "Webhook")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
public class WebhookItemControllerBase : ManagementApiControllerBase
{
}
