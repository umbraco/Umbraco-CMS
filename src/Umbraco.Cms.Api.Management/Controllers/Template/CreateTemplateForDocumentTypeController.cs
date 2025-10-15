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

[ApiVersion("1.0")]
public class CreateTemplateForDocumentTypeController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateTemplateForDocumentTypeController(
        ITemplateService templateService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _templateService = templateService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("for-document-type")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForDocumentType(CancellationToken cancellationToken, CreateTemplateForDocumentTypeRequestModel requestModel)
    {
        Attempt<ITemplate?, TemplateOperationStatus> result = await _templateService.CreateForContentTypeAsync(
            requestModel.DocumentType.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyTemplateController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : TemplateOperationStatusResult(result.Status);
    }
}
