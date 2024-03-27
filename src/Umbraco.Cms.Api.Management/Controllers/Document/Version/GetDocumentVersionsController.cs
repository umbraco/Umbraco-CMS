using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Version;

[ApiVersion("1.0")]
public class GetDocumentVersionsController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IDocumentVersionPresentationFactory _documentVersionPresentationFactory;

    public GetDocumentVersionsController(
        IContentVersionService contentVersionService,
        IDocumentVersionPresentationFactory documentVersionPresentationFactory)
    {
        _contentVersionService = contentVersionService;
        _documentVersionPresentationFactory = documentVersionPresentationFactory;
    }

    // move to item?
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedViewModel<DocumentVersionItemResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(Guid documentId, string? culture, int skip = 0, int take = 100)
    {
        // old ContentController.GetRollbackVersions()
        // get all versions for a given document
        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> attempt =
            await _contentVersionService.GetContentVersionsAsync(documentId, culture, skip, take);

        return attempt.Success is true
            ? Ok(await _documentVersionPresentationFactory.CreatedPagedResponseModelAsync(attempt.Result!))
            : MapFailure(attempt.Status);
    }
}
