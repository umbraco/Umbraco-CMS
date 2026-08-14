using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Item;

/// <summary>
/// Provides API endpoints for searching member type items in the management interface.
/// </summary>
[ApiVersion("1.0")]
public class SearchMemberTypeItemController : MemberTypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchMemberTypeItemController"/> class, which handles search operations for member type items.
    /// </summary>
    /// <param name="entitySearchService">Service used to perform entity search operations.</param>
    /// <param name="memberTypeService">Service used to manage member types.</param>
    /// <param name="mapper">The Umbraco mapper used for mapping entities to models.</param>
    public SearchMemberTypeItemController(IEntitySearchService entitySearchService, IMemberTypeService memberTypeService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _memberTypeService = memberTypeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Searches for member type items matching the specified query, with support for pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query used to filter member type items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedModel{MemberTypeItemResponseModel}"/> containing the search results.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MemberTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches member type items.")]
    [EndpointDescription("Searches member type items by the provided query with pagination support.")]
    public Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.MemberType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Task.FromResult<IActionResult>(Ok(new PagedModel<MemberTypeItemResponseModel> { Total = searchResult.Total }));
        }

        Guid[] keys = searchResult.Items.Select(item => item.Key).ToArray();
        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(keys);
        IEnumerable<IMemberType> orderedMemberTypes = OrderByRequestedIds(memberTypes, keys);

        var result = new PagedModel<MemberTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IMemberType, MemberTypeItemResponseModel>(orderedMemberTypes),
            Total = searchResult.Total,
        };

        return Task.FromResult<IActionResult>(Ok(result));
    }
}
