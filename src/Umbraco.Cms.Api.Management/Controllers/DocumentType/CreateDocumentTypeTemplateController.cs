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

[ApiVersion("1.0")]
public class CreateDocumentTypeTemplateController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateDocumentTypeTemplateController(
        IContentTypeService contentTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentTypeService = contentTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("{id:guid}/template")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
