using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

/// <summary>
/// API controller responsible for handling requests to update member groups in the system.
/// </summary>
[ApiVersion("1.0")]
public class UpdateMemberGroupController : MemberGroupControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberGroupService _memberGroupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMemberGroupController"/> class, which handles API requests for updating member groups.
    /// </summary>
    /// <param name="mapper">The Umbraco object mapper used for mapping between API models and domain models.</param>
    /// <param name="memberGroupService">The service used to manage member group operations.</param>
    public UpdateMemberGroupController(IUmbracoMapper mapper, IMemberGroupService memberGroupService)
    {
        _mapper = mapper;
        _memberGroupService = memberGroupService;
    }

    /// <summary>
    /// Updates the specified member group with new details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member group to update.</param>
    /// <param name="model">The model containing the updated member group details.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the update operation.</returns>
    [HttpPut($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a member group.")]
    [EndpointDescription("Updates a member group identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateMemberGroupRequestModel model)
    {
        IMemberGroup? current = await _memberGroupService.GetAsync(id);
        if (current is null)
        {
            return MemberGroupNotFound();
        }

        IMemberGroup updated = _mapper.Map(model, current);

        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await _memberGroupService.UpdateAsync(updated);
        return result.Success
            ? Ok()
            : MemberGroupOperationStatusResult(result.Status);
    }
}
