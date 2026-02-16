using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Item;

[ApiVersion("1.0")]
public class SearchTemplateItemController : TemplateItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _mapper;
    private readonly ISearchResultAncestorService _searchResultAncestorService;

    [ActivatorUtilitiesConstructor]
    public SearchTemplateItemController(
        IEntitySearchService entitySearchService,
        ITemplateService templateService,
        IUmbracoMapper mapper,
        ISearchResultAncestorService searchResultAncestorService)
    {
        _entitySearchService = entitySearchService;
        _templateService = templateService;
        _mapper = mapper;
        _searchResultAncestorService = searchResultAncestorService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public SearchTemplateItemController(IEntitySearchService entitySearchService, ITemplateService templateService, IUmbracoMapper mapper)
        : this(
            entitySearchService,
            templateService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<ISearchResultAncestorService>())
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<SearchTemplateItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches template items.")]
    [EndpointDescription("Searches template items by the provided query with pagination support.")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.Template, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<SearchTemplateItemResponseModel> { Total = searchResult.Total });
        }

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> ancestorsByKey =
            await _searchResultAncestorService.ResolveAsync(searchResult.Items, UmbracoObjectTypes.Template);

        IEnumerable<ITemplate> templates = await _templateService.GetAllAsync(searchResult.Items.Select(item => item.Key).ToArray());
        IEnumerable<SearchTemplateItemResponseModel> searchModels = _mapper.MapEnumerable<ITemplate, SearchTemplateItemResponseModel>(templates);

        foreach (SearchTemplateItemResponseModel model in searchModels)
        {
            if (ancestorsByKey.TryGetValue(model.Id, out IReadOnlyList<SearchResultAncestorModel>? ancestors))
            {
                model.Ancestors = ancestors;
            }
        }

        var result = new PagedModel<SearchTemplateItemResponseModel>
        {
            Items = searchModels,
            Total = searchResult.Total
        };

        return Ok(result);
    }
}
