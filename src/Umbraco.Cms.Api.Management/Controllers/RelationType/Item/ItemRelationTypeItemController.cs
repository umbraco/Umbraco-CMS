using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RelationType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Item;

/// <summary>
/// Provides API endpoints for managing relation type items within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemRelationTypeItemController : RelationTypeItemControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemRelationTypeItemController"/> class.
    /// </summary>
    /// <param name="relationService">The service used to manage relations between entities.</param>
    /// <param name="mapper">The mapper used for mapping Umbraco objects to API models.</param>
    public ItemRelationTypeItemController(IRelationService relationService, IUmbracoMapper mapper)
    {
        _relationService = relationService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a collection of relation type items matching the specified IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of relation type item IDs to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with the collection of matching relation type items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<RelationTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of relation type items.")]
    [EndpointDescription("Gets a collection of relation type items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<RelationTypeItemResponseModel>()));
        }

        // relation service does not allow fetching a collection of relation types by their ids; instead it relies
        // heavily on caching, which means this is as fast as it gets - even if it looks less than performant
        IRelationType[] relationTypes = _relationService
            .GetAllRelationTypes()
            .Where(relationType => ids.Contains(relationType.Key)).ToArray();

        List<RelationTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IRelationType, RelationTypeItemResponseModel>(relationTypes);

        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
