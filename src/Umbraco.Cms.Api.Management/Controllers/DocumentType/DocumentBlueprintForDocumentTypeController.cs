using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class DocumentBlueprintForDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;

    private readonly IUmbracoMapper _umbracoMapper;

    public DocumentBlueprintForDocumentTypeController(IContentBlueprintEditingService contentBlueprintEditingService, IUmbracoMapper umbracoMapper)
    {
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/blueprint")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeBlueprintItemResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DocumentBlueprintByDocumentTypeKey(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<IContent>?, ContentEditingOperationStatus> attempt = await _contentBlueprintEditingService.GetPagedByContentTypeAsync(id, skip, take);
        if (attempt.Success is false)
        {
            return ContentEditingOperationStatusResult(attempt.Status);
        }

        List<DocumentTypeBlueprintItemResponseModel> viewModels = _umbracoMapper.MapEnumerable<IContent, DocumentTypeBlueprintItemResponseModel>(attempt.Result!.Items);

        var pagedViewModel = new PagedViewModel<DocumentTypeBlueprintItemResponseModel>
        {
            Total = attempt.Result!.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
