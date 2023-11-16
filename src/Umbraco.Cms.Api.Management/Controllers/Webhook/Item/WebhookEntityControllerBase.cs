using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Webhook}")]
[ApiExplorerSettings(GroupName = "Webhook")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessWebhooks)]
public class WebhookEntityControllerBase : ManagementApiControllerBase
{
}
