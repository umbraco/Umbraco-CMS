using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Item;

[ApiVersion("1.0")]
public class SearchMemberTypeItemController : MemberTypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _mapper;
    private readonly ISearchResultAncestorService _searchResultAncestorService;

    [ActivatorUtilitiesConstructor]
    public SearchMemberTypeItemController(
        IEntitySearchService entitySearchService,
        IMemberTypeService memberTypeService,
        IUmbracoMapper mapper,
        ISearchResultAncestorService searchResultAncestorService)
    {
        _entitySearchService = entitySearchService;
        _memberTypeService = memberTypeService;
        _mapper = mapper;
        _searchResultAncestorService = searchResultAncestorService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public SearchMemberTypeItemController(IEntitySearchService entitySearchService, IMemberTypeService memberTypeService, IUmbracoMapper mapper)
        : this(
            entitySearchService,
            memberTypeService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<ISearchResultAncestorService>())
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<SearchMemberTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches member type items.")]
    [EndpointDescription("Searches member type items by the provided query with pagination support.")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.MemberType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<SearchMemberTypeItemResponseModel> { Total = searchResult.Total });
        }

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> ancestorsByKey =
            await _searchResultAncestorService.ResolveAsync(searchResult.Items, UmbracoObjectTypes.MemberType);

        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(searchResult.Items.Select(item => item.Key).ToArray());
        IEnumerable<SearchMemberTypeItemResponseModel> searchModels = _mapper.MapEnumerable<IMemberType, SearchMemberTypeItemResponseModel>(memberTypes);

        foreach (SearchMemberTypeItemResponseModel model in searchModels)
        {
            if (ancestorsByKey.TryGetValue(model.Id, out IReadOnlyList<SearchResultAncestorModel>? ancestors))
            {
                model.Ancestors = ancestors;
            }
        }

        var result = new PagedModel<SearchMemberTypeItemResponseModel>
        {
            Items = searchModels,
            Total = searchResult.Total
        };

        return Ok(result);
    }
}
