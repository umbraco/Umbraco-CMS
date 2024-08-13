using System.ComponentModel.DataAnnotations;
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

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

[ApiVersion("1.0")]
public class AllDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IDocumentVersionPresentationFactory _documentVersionPresentationFactory;

    public AllDocumentVersionController(
        IContentVersionService contentVersionService,
        IDocumentVersionPresentationFactory documentVersionPresentationFactory)
    {
        _contentVersionService = contentVersionService;
        _documentVersionPresentationFactory = documentVersionPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedViewModel<DocumentVersionItemResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> All(
        CancellationToken cancellationToken,
        [Required] Guid documentId,
        string? culture,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> attempt =
            await _contentVersionService.GetPagedContentVersionsAsync(documentId, culture, skip, take);

        var pagedViewModel = new PagedViewModel<DocumentVersionItemResponseModel>
        {
            Total = attempt.Result!.Total,
            Items = await _documentVersionPresentationFactory.CreateMultipleAsync(attempt.Result!.Items),
        };

        return attempt.Success
            ? Ok(pagedViewModel)
            : MapFailure(attempt.Status);
    }
}
