using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Logs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Logs;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Webhook}")]
[ApiExplorerSettings(GroupName = "Webhook")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
public class WebhookLogControllerBase : ManagementApiControllerBase
{
    protected PagedViewModel<WebhookLogResponseModel> CreatePagedWebhookLogResponseModel(PagedModel<WebhookLog> logs, IWebhookPresentationFactory webhookPresentationFactory)
    {
        WebhookLogResponseModel[] logResponseModels = logs.Items.Select(webhookPresentationFactory.CreateResponseModel).ToArray();

        return new PagedViewModel<WebhookLogResponseModel>
        {
            Total = logs.Total,
            Items = logResponseModels,
        };
    }

}
