using Asp.Versioning;
using J2N.Collections.Generic.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Item;

/// <summary>
/// API controller responsible for managing member group items within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class ItemMemberGroupItemController : MemberGroupItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MemberGroup.Item.ItemMemberGroupItemController"/> class, providing services for managing member group items.
    /// </summary>
    /// <param name="entityService">The service used to interact with entities in the Umbraco CMS.</param>
    /// <param name="mapper">The mapper used for mapping Umbraco objects.</param>
    public ItemMemberGroupItemController(IEntityService entityService, IUmbracoMapper mapper)
    {
        _entityService = entityService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberGroupItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member group items.")]
    [EndpointDescription("Gets a collection of member group items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<MemberGroupItemResponseModel>()));
        }

        IEnumerable<IEntitySlim> memberGroups = _entityService.GetAll(UmbracoObjectTypes.MemberGroup, ids.ToArray());
        List<MemberGroupItemResponseModel> responseModel = _mapper.MapEnumerable<IEntitySlim, MemberGroupItemResponseModel>(memberGroups);
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
