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
public class UpdateWebhookController : WebhookControllerBase
{
    private readonly IWebHookService _webhookService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateWebhookController(
        IWebHookService webhookService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webhookService = webhookService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut($"{{{nameof(id)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateWebhookRequestModel updateWebhookRequestModel)
    {
        IWebhookEvent? current = await _webhookService.GetAsync(id);
        if (current is null)
        {
            return WebhookNotFound();
        }

        IWebhookEvent updated = _umbracoMapper.Map(updateWebhookRequestModel, current);

        Attempt<ILanguage, LanguageOperationStatus> result = await _webhookService.UpdateAsync(updated, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : WebhookOperationStatusResult(result.Status);
    }
}
