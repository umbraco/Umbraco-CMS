using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

/// <summary>
/// Provides API endpoints for managing member groups by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyMemberGroupController"/> class with the specified member group service and Umbraco mapper.
    /// </summary>
    /// <param name="memberGroupService">An instance of <see cref="IMemberGroupService"/> used to manage member groups.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping entities.</param>
    public ByKeyMemberGroupController(IMemberGroupService memberGroupService, IUmbracoMapper mapper)
    {
        _memberGroupService = memberGroupService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a member group by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the member group to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="MemberGroupResponseModel"/> if the member group is found; otherwise, a 404 Not Found response.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a member group.")]
    [EndpointDescription("Gets a member group identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IMemberGroup? memberGroup = await _memberGroupService.GetAsync(id);
        if (memberGroup is null)
        {
            return MemberGroupNotFound();
        }

        MemberGroupResponseModel responseModel = _mapper.Map<MemberGroupResponseModel>(memberGroup)!;

        return Ok(responseModel);
    }
}
