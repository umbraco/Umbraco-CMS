using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class SortDocumentController : DocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public SortDocumentController(IContentEditingService contentEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("sort")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sort(SortingRequestModel sortingRequestModel)
    {
        ContentEditingOperationStatus result = await _contentEditingService.SortAsync(
            sortingRequestModel.ParentId,
            sortingRequestModel.Sorting.Select(m => new SortingModel { Key = m.Id, SortOrder = m.SortOrder }),
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result == ContentEditingOperationStatus.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result);
    }
}
