using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class AllowedChildrenDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllowedChildrenDocumentTypeController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/allowed-children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedDocumentType>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedChildrenByKey(
        CancellationToken cancellationToken,
        Guid id,
        Guid? parentContentKey = null,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<IContentType>?, ContentTypeOperationStatus> attempt = await _contentTypeService.GetAllowedChildrenAsync(id, parentContentKey, skip, take);
        if (attempt.Success is false)
        {
            return OperationStatusResult(attempt.Status);
        }

        List<AllowedDocumentType> viewModels = _umbracoMapper.MapEnumerable<IContentType, AllowedDocumentType>(attempt.Result!.Items);

        var pagedViewModel = new PagedViewModel<AllowedDocumentType>
        {
            Total = attempt.Result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
