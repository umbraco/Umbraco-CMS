using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

/// <summary>
/// API controller responsible for managing operations related to all member groups.
/// Provides endpoints for retrieving, creating, updating, and deleting member groups.
/// </summary>
[ApiVersion("1.0")]
public class AllMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllMemberGroupController"/> class.
    /// </summary>
    /// <param name="memberGroupService">Service used to manage member groups.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    public AllMemberGroupController(IMemberGroupService memberGroupService, IUmbracoMapper mapper)
    {
        _memberGroupService = memberGroupService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberGroupResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of member groups.")]
    [EndpointDescription("Gets a paginated collection of all member groups.")]
    public async Task<ActionResult<PagedViewModel<MemberGroupResponseModel>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        IMemberGroup[] memberGroups = (await _memberGroupService.GetAllAsync()).OrderBy(g => g.Name).ToArray();
        var viewModel = new PagedViewModel<MemberGroupResponseModel>
        {
            Total = memberGroups.Length,
            Items = _mapper.MapEnumerable<IMemberGroup, MemberGroupResponseModel>(memberGroups.Skip(skip).Take(take)),
        };

        return Ok(viewModel);
    }
}
