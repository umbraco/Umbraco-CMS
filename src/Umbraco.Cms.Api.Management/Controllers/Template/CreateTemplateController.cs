using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[ApiVersion("1.0")]
public class CreateTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    [ActivatorUtilitiesConstructor]
    public CreateTemplateController(
        ITemplateService templateService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IOptions<RuntimeSettings> runtimeSettings)
    {
        _templateService = templateService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _runtimeSettings = runtimeSettings;
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public CreateTemplateController(
        ITemplateService templateService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            templateService,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<RuntimeSettings>>())
    {
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new template.")]
    [EndpointDescription("Creates a new template with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateTemplateRequestModel requestModel)
    {
        if (_runtimeSettings.Value.Mode == RuntimeMode.Production)
        {
            return TemplateOperationStatusResult(TemplateOperationStatus.NotAllowedInProductionMode);
        }

        Attempt<ITemplate, TemplateOperationStatus> result = await _templateService.CreateAsync(
            requestModel.Name,
            requestModel.Alias,
            requestModel.Content,
            CurrentUserKey(_backOfficeSecurityAccessor),
            requestModel.Id);

        return result.Success
            ? CreatedAtId<ByKeyTemplateController>(controller => nameof(controller.ByKey), result.Result.Key)
            : TemplateOperationStatusResult(result.Status);
    }
}
