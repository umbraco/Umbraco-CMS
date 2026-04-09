using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[ApiVersion("1.0")]
public class UpdateTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    [ActivatorUtilitiesConstructor]
    public UpdateTemplateController(
        ITemplateService templateService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IOptions<RuntimeSettings> runtimeSettings)
    {
        _templateService = templateService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _runtimeSettings = runtimeSettings;
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public UpdateTemplateController(
        ITemplateService templateService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            templateService,
            umbracoMapper,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<RuntimeSettings>>())
    {
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a template.")]
    [EndpointDescription("Updates a template identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateTemplateRequestModel requestModel)
    {
        ITemplate? template = await _templateService.GetAsync(id);
        if (template == null)
        {
            return TemplateNotFound();
        }

        // In production mode, block updates if the content is being changed.
        var existingContent = template.Content;
        template = _umbracoMapper.Map(requestModel, template);
        if (_runtimeSettings.Value.Mode == RuntimeMode.Production && existingContent != template.Content)
        {
            return TemplateOperationStatusResult(TemplateOperationStatus.ContentChangeNotAllowedInProductionMode);
        }

        Attempt<ITemplate, TemplateOperationStatus> result = await _templateService.UpdateAsync(template, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success ?
            Ok()
            : TemplateOperationStatusResult(result.Status);
    }
}
