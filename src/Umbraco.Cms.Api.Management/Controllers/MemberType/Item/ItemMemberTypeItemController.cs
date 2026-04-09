using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Item;

/// <summary>
/// API controller responsible for handling operations related to individual member type items in the management section.
/// </summary>
[ApiVersion("1.0")]
public class ItemMemberTypeItemController : MemberTypeItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemMemberTypeItemController"/> class, which manages member type items in the Umbraco CMS API.
    /// </summary>
    /// <param name="mapper">The Umbraco mapper used for mapping between different object models.</param>
    /// <param name="memberTypeService">The service used to manage member types.</param>
    public ItemMemberTypeItemController(IUmbracoMapper mapper, IMemberTypeService memberTypeService)
    {
        _mapper = mapper;
        _memberTypeService = memberTypeService;
    }

    /// <summary>
    /// Retrieves member type items corresponding to the specified unique identifiers.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of unique identifiers for the member type items to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="MemberTypeItemResponseModel"/> objects.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member type items.")]
    [EndpointDescription("Gets a collection of member type items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<MemberTypeItemResponseModel>()));
        }

        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(ids);
        List<MemberTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IMemberType, MemberTypeItemResponseModel>(memberTypes);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
