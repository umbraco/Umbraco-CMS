using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[ApiVersion("1.0")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessWebhooks)]
public class CreateWebhookController : WebhookControllerBase
{
    private readonly IWebHookService _webhookService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateWebhookController(
        IWebHookService webhookService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webhookService = webhookService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateWebhookRequestModel createWebhookRequestModel)
    {
        IWebhookEvent created = _umbracoMapper.Map<IWebhookEvent>(createWebhookRequestModel)!;

        Attempt<IWebhookEvent, WebhookOperationStatus> result = await _webhookService.CreateAsync(created)); //, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByIsoCodeLanguageController>(controller => nameof(controller.ByIsoCode), new { isoCode = result.Result.IsoCode })
            : WebhookOperationStatusResult(result.Status);
    }
}
