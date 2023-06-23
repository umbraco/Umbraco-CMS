using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class AllowedChildrenByKeyDocumentController : DocumentControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllowedChildrenByKeyDocumentController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/allowed-document-types")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedChildrenByKey(Guid id, int skip = 0, int take = 100)
    {
        IContentType? documentType = _contentTypeService.Get(id);

        if (documentType?.AllowedContentTypes is null)
        {
            return NotFound();
        }

        PagedModel<IContentType> allowedChildren = await _contentTypeService.GetAllAsync(documentType.AllowedContentTypes.Select(x => x.Key), skip, take);

        List<DocumentTypeResponseModel> viewModels = _umbracoMapper.MapEnumerable<IContentType, DocumentTypeResponseModel>(allowedChildren.Items);

        var pagedViewModel = new PagedViewModel<DocumentTypeResponseModel>
        {
            Total = allowedChildren.Total,
            Items = viewModels,
        };

        return await Task.FromResult(Ok(pagedViewModel));
    }
}
