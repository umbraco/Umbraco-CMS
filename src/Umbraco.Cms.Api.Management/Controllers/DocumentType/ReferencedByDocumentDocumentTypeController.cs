using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ReferencedByDocumentDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeReferenceService _contentTypeReferenceService;

    public ReferencedByDocumentDocumentTypeController(IContentTypeReferenceService contentTypeReferenceService) => _contentTypeReferenceService = contentTypeReferenceService;

    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReferencedBy(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 100)
    {
        PagedModel<Guid> documentKeys = await _contentTypeReferenceService.GetReferencedDocumentKeysAsync(id, cancellationToken, skip, take);

        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = documentKeys.Total,
            Items = documentKeys.Items.Select(x => new ReferenceByIdModel(x)),
        };

        return Ok(pagedViewModel);
    }

}
