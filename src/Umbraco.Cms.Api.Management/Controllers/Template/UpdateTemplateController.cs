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

/// <summary>
/// API controller responsible for handling requests to update templates in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class UpdateTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTemplateController"/> class, which manages update operations for templates in the Umbraco CMS.
    /// </summary>
    /// <param name="templateService">Service used to perform operations on templates.</param>
    /// <param name="umbracoMapper">Mapper used to convert between domain models and API models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="runtimeSettings">The runtime configuration settings.</param>
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

    /// <summary>
    /// Updates an existing template with the specified ID using the provided request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the template to update.</param>
    /// <param name="requestModel">The model containing the updated template details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
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
