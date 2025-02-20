using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

[ApiVersion("1.0")]
public class SearchDocumentTypeItemController : DocumentTypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _mapper;

    public SearchDocumentTypeItemController(IEntitySearchService entitySearchService, IContentTypeService contentTypeService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _contentTypeService = contentTypeService;
        _mapper = mapper;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.DocumentType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return await Task.FromResult(Ok(new PagedModel<DocumentTypeItemResponseModel> { Total = searchResult.Total }));
        }

        IEnumerable<IContentType> contentTypes = _contentTypeService.GetMany(searchResult.Items.Select(item => item.Key).ToArray().EmptyNull());
        var result = new PagedModel<DocumentTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IContentType, DocumentTypeItemResponseModel>(contentTypes),
            Total = searchResult.Total
        };

        return Ok(result);
    }
}
