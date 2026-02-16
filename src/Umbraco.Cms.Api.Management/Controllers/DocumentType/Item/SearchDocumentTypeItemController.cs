using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Item;
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
    private readonly ISearchResultAncestorService _searchResultAncestorService;

    [ActivatorUtilitiesConstructor]
    public SearchDocumentTypeItemController(
        IUmbracoMapper mapper,
        IContentTypeSearchService contentTypeSearchService,
        ISearchResultAncestorService searchResultAncestorService)
    {
        _mapper = mapper;
        _contentTypeSearchService = contentTypeSearchService;
        _searchResultAncestorService = searchResultAncestorService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public SearchDocumentTypeItemController(IUmbracoMapper mapper, IContentTypeSearchService contentTypeSearchService)
        : this(
            mapper,
            contentTypeSearchService,
            StaticServiceProvider.Instance.GetRequiredService<ISearchResultAncestorService>())
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<SearchDocumentTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches document type items.")]
    [EndpointDescription("Searches document type items by the provided query with pagination support.")]
    public async Task<IActionResult> SearchDocumentType(CancellationToken cancellationToken, string query, bool? isElement = null, int skip = 0, int take = 100)
    {
        PagedModel<IContentType> contentTypes = await _contentTypeSearchService.SearchAsync(query, isElement, cancellationToken, skip, take);

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> ancestorsByKey =
            await _searchResultAncestorService.ResolveAsync(contentTypes.Items, UmbracoObjectTypes.DocumentType);

        IEnumerable<SearchDocumentTypeItemResponseModel> searchModels = _mapper.MapEnumerable<IContentType, SearchDocumentTypeItemResponseModel>(contentTypes.Items);

        foreach (SearchDocumentTypeItemResponseModel model in searchModels)
        {
            if (ancestorsByKey.TryGetValue(model.Id, out IReadOnlyList<SearchResultAncestorModel>? ancestors))
            {
                model.Ancestors = ancestors;
            }
        }

        var result = new PagedModel<SearchDocumentTypeItemResponseModel>
        {
            Items = searchModels,
            Total = contentTypes.Total
        };

        return Ok(result);
    }
}
