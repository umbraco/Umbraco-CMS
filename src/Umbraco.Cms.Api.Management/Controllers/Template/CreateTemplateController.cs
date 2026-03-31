using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

/// <summary>
/// Provides API endpoints for creating templates within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class CreateTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTemplateController"/> class.
    /// </summary>
    /// <param name="templateService">An instance of <see cref="ITemplateService"/> used to manage templates.</param>
    /// <param name="backOfficeSecurityAccessor">An instance of <see cref="IBackOfficeSecurityAccessor"/> used to access back office security information.</param>
    public CreateTemplateController(
        ITemplateService templateService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _templateService = templateService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new template using the specified request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="requestModel">The details of the template to create.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new template.")]
    [EndpointDescription("Creates a new template with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateTemplateRequestModel requestModel)
    {
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
