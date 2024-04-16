using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
public class CreateWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IUmbracoMapper _umbracoMapper;

    public CreateWebhookController(
        IWebhookService webhookService, IUmbracoMapper umbracoMapper)
    {
        _webhookService = webhookService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateWebhookRequestModel createWebhookRequestModel)
    {
        IWebhook created = _umbracoMapper.Map<IWebhook>(createWebhookRequestModel)!;

        Attempt<IWebhook, WebhookOperationStatus> result = await _webhookService.CreateAsync(created);

        return result.Success
            ? CreatedAtId<ByKeyWebhookController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : WebhookOperationStatusResult(result.Status);
    }
}
