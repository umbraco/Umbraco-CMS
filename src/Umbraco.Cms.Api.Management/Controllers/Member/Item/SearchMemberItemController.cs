using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiVersion("1.0")]
public class SearchMemberItemController : MemberItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;
    private readonly ISearchResultAncestorService _searchResultAncestorService;

    [ActivatorUtilitiesConstructor]
    public SearchMemberItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IMemberPresentationFactory memberPresentationFactory,
        ISearchResultAncestorService searchResultAncestorService)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _memberPresentationFactory = memberPresentationFactory;
        _searchResultAncestorService = searchResultAncestorService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public SearchMemberItemController(IIndexedEntitySearchService indexedEntitySearchService, IMemberPresentationFactory memberPresentationFactory)
        : this(
            indexedEntitySearchService,
            memberPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<ISearchResultAncestorService>())
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<SearchMemberItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches member items.")]
    [EndpointDescription("Searches member items by the provided query with pagination support.")]
    public async Task<IActionResult> SearchWithAllowedTypes(CancellationToken cancellationToken, string query, int skip = 0, int take = 100, [FromQuery]IEnumerable<Guid>? allowedMemberTypes = null)
    {
        PagedModel<IEntitySlim> searchResult = await _indexedEntitySearchService.SearchAsync(UmbracoObjectTypes.Member, query, null, allowedMemberTypes, false, "*", skip, take);

        IMemberEntitySlim[] memberEntities = searchResult.Items.OfType<IMemberEntitySlim>().ToArray();

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> ancestorsByKey =
            await _searchResultAncestorService.ResolveAsync(memberEntities, UmbracoObjectTypes.Member);

        SearchMemberItemResponseModel[] items = memberEntities
            .Select(entity =>
                _memberPresentationFactory.CreateSearchItemResponseModel(
                    entity,
                    ancestorsByKey.TryGetValue(entity.Key, out IReadOnlyList<SearchResultAncestorModel>? ancestors)
                        ? ancestors
                        : []))
            .ToArray();

        var result = new PagedModel<SearchMemberItemResponseModel>
        {
            Items = items,
            Total = searchResult.Total,
        };

        return Ok(result);
    }
}
