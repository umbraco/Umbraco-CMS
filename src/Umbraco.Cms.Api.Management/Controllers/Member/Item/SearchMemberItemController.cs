using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

    /// <summary>
    /// Provides API endpoints for searching member items within the management interface.
    /// </summary>
[ApiVersion("1.0")]
public class SearchMemberItemController : MemberItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Member.Item.SearchMemberItemController"/> class.
    /// </summary>
    /// <param name="indexedEntitySearchService">Service for performing indexed entity search operations.</param>
    /// <param name="memberPresentationFactory">Factory for creating member presentation models.</param>
    public SearchMemberItemController(IIndexedEntitySearchService indexedEntitySearchService, IMemberPresentationFactory memberPresentationFactory)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MemberItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches member items.")]
    [EndpointDescription("Searches member items by the provided query with pagination support.")]
    public async Task<IActionResult> SearchWithAllowedTypes(CancellationToken cancellationToken, string query, int skip = 0, int take = 100, [FromQuery]IEnumerable<Guid>? allowedMemberTypes = null)
    {
        PagedModel<IEntitySlim> searchResult = await _indexedEntitySearchService.SearchAsync(UmbracoObjectTypes.Member, query, null, allowedMemberTypes, false, "*", skip, take);
        var result = new PagedModel<MemberItemResponseModel>
        {
            Items = searchResult.Items.OfType<IMemberEntitySlim>().Select(_memberPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total,
        };

        return await Task.FromResult<IActionResult>(Ok(result));
    }
}
