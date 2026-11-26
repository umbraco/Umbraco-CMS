using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

/// <summary>
/// Provides API endpoints for searching member items within the management interface.
/// </summary>
[ApiVersion("1.0")]
public class SearchMemberItemController : MemberItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMemberPresentationService _memberPresentationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Member.Item.SearchMemberItemController"/> class.
    /// </summary>
    /// <param name="indexedEntitySearchService">Service for performing indexed entity search operations.</param>
    /// <param name="memberPresentationService">Service for creating member item presentation models, for content-based and external members alike.</param>
    [ActivatorUtilitiesConstructor]
    public SearchMemberItemController(IIndexedEntitySearchService indexedEntitySearchService, IMemberPresentationService memberPresentationService)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _memberPresentationService = memberPresentationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Member.Item.SearchMemberItemController"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor exists only to give the dependency injection container an unambiguous (greediest) constructor
    /// to select while the obsolete constructor remains. Use the constructor taking an <see cref="IMemberPresentationService"/>.
    /// </remarks>
    /// <param name="indexedEntitySearchService">Service for performing indexed entity search operations.</param>
    /// <param name="memberPresentationFactory">Unused.</param>
    /// <param name="memberPresentationService">Service for creating member item presentation models, for content-based and external members alike.</param>
    [Obsolete("This constructor exists only to satisfy dependency injection. Please use the constructor taking an IMemberPresentationService. Scheduled for removal in Umbraco 21.")]
    public SearchMemberItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IMemberPresentationFactory memberPresentationFactory,
        IMemberPresentationService memberPresentationService)
        : this(indexedEntitySearchService, memberPresentationService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Member.Item.SearchMemberItemController"/> class.
    /// </summary>
    /// <param name="indexedEntitySearchService">Service for performing indexed entity search operations.</param>
    /// <param name="memberPresentationFactory">Factory for creating member presentation models.</param>
    [Obsolete("Use the constructor taking an IMemberPresentationService. Scheduled for removal in Umbraco 21.")]
    public SearchMemberItemController(IIndexedEntitySearchService indexedEntitySearchService, IMemberPresentationFactory memberPresentationFactory)
        : this(
            indexedEntitySearchService,
            StaticServiceProvider.Instance.GetRequiredService<IMemberPresentationService>())
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MemberItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches member items.")]
    [EndpointDescription("Searches member items by the provided query with pagination support.")]
    public async Task<IActionResult> SearchWithAllowedTypes(CancellationToken cancellationToken, string query, int skip = 0, int take = 100, [FromQuery]IEnumerable<Guid>? allowedMemberTypes = null)
    {
        // the members index also holds external members, which are not entities, so the matching keys are
        // resolved by the member presentation service rather than as entities
        PagedModel<Guid> searchResult = await _indexedEntitySearchService.SearchKeysAsync(UmbracoObjectTypes.Member, query, null, allowedMemberTypes, false, "*", skip, take);
        Guid[] keys = searchResult.Items.ToArray();
        if (keys.Length is 0)
        {
            return Ok(new PagedModel<MemberItemResponseModel> { Total = searchResult.Total });
        }

        IEnumerable<MemberItemResponseModel> items = await _memberPresentationService.CreateItemResponseModelsAsync(keys.ToHashSet());
        var result = new PagedModel<MemberItemResponseModel>
        {
            Items = items.OrderBy(item => Array.IndexOf(keys, item.Id)).ToArray(),
            Total = searchResult.Total,
        };

        return Ok(result);
    }
}
