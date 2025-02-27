using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Logs;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Webhook}")]
[ApiExplorerSettings(GroupName = "Webhook")]
public class WebhookLogControllerBase : ManagementApiControllerBase;
