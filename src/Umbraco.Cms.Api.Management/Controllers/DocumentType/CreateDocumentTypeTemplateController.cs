using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Template;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Controller responsible for handling requests to create templates for document types in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class CreateDocumentTypeTemplateController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentTypeTemplateController"/> class.
    /// </summary>
    /// <param name="contentTypeService">Service used for managing content types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CreateDocumentTypeTemplateController(
        IContentTypeService contentTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentTypeService = contentTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a template for the specified document type.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document type.</param>
    /// <param name="requestModel">The details of the template to create.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPost("{id:guid}/template")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Creates a template for a document type.")]
    [EndpointDescription("Creates a new template associated with the document type identified by the provided Id.")]
    public async Task<IActionResult> CreateTemplate(
        CancellationToken cancellationToken,
        Guid id,
        CreateDocumentTypeTemplateRequestModel requestModel)
    {
        Attempt<Guid?, ContentTypeOperationStatus> result = await _contentTypeService.CreateTemplateAsync(
            id,
            requestModel.Name,
            requestModel.Alias,
            requestModel.IsDefault,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyTemplateController>(controller => nameof(controller.ByKey), result.Result!.Value)
            : OperationStatusResult(result.Status);
    }
}
