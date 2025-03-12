using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

[ApiVersion("1.0")]
public class SearchDocumentTypeItemController : DocumentTypeItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IContentTypeSearchService _contentTypeSearchService;

    [Obsolete("Please use ctor that only accepts IUmbracoMapper & IContentTypeSearchService, scheduled for removal in v17")]
    public SearchDocumentTypeItemController(IEntitySearchService entitySearchService, IContentTypeService contentTypeService, IUmbracoMapper mapper)
    : this(mapper, StaticServiceProvider.Instance.GetRequiredService<IContentTypeSearchService>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public SearchDocumentTypeItemController(IUmbracoMapper mapper, IContentTypeSearchService contentTypeSearchService)
    {
        _mapper = mapper;
        _contentTypeSearchService = contentTypeSearchService;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, bool? isElement = null, int skip = 0, int take = 100)
    {
        PagedModel<IContentType> contentTypes = await _contentTypeSearchService.SearchAsync(query, isElement, cancellationToken, skip, take);
        var result = new PagedModel<DocumentTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IContentType, DocumentTypeItemResponseModel>(contentTypes.Items),
            Total = contentTypes.Total
        };

        return Ok(result);
    }
}
