using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Item;

/// <summary>
/// API controller responsible for managing member group items within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class ItemMemberGroupItemController : MemberGroupItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberGroupService _memberGroupService;

    // TODO (V19): When the obsolete constructor is removed, also remove the unused dependency on IEntityService.

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemMemberGroupItemController"/> class.
    /// </summary>
    /// <param name="mapper">The mapper used for mapping Umbraco objects.</param>
    /// <param name="memberGroupService">The service used to look up member groups.</param>
    [ActivatorUtilitiesConstructor]
    public ItemMemberGroupItemController(IEntityService entityService, IUmbracoMapper mapper, IMemberGroupService memberGroupService)
    {
        _mapper = mapper;
        _memberGroupService = memberGroupService;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ItemMemberGroupItemController(IEntityService entityService, IUmbracoMapper mapper)
        : this(
            entityService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<IMemberGroupService>())
    {
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberGroupItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member group items.")]
    [EndpointDescription("Gets a collection of member group items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<MemberGroupItemResponseModel>());
        }

        // Resolve via IMemberGroupService so custom implementations are honoured, rather than
        // going directly to the entity/repository layer.
        IEnumerable<IMemberGroup> memberGroups = await _memberGroupService.GetAsync(ids);
        List<MemberGroupItemResponseModel> responseModel = _mapper.MapEnumerable<IMemberGroup, MemberGroupItemResponseModel>(memberGroups);
        return Ok(responseModel);
    }
}
