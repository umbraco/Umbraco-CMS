using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.ElementDraftBranch;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API endpoint for scheduling (or clearing the schedule of) an element draft branch's automatic
/// publish.
/// </summary>
[ApiVersion("1.0")]
public class ScheduleElementDraftBranchController : ElementDraftBranchControllerBase
{
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleElementDraftBranchController"/> class.
    /// </summary>
    public ScheduleElementDraftBranchController(
        IElementDraftBranchService elementDraftBranchService,
        IAuthorizationService authorizationService)
    {
        _elementDraftBranchService = elementDraftBranchService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Sets or clears an element draft branch's scheduled publish date.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to schedule.</param>
    /// <param name="requestModel">The publish date to schedule, or <c>null</c> to clear an existing schedule.</param>
    [HttpPut("{branchId:guid}/schedule")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Schedules an element draft branch.")]
    [EndpointDescription("Sets or clears the scheduled publish date of an element draft branch identified by the provided Id.")]
    public async Task<IActionResult> Schedule(
        CancellationToken cancellationToken,
        Guid id,
        Guid branchId,
        ScheduleElementDraftBranchRequestModel requestModel)
    {
        IContentDraftBranch? branch = _elementDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementUpdate.ActionLetter, branch.ContentKey, requestModel.Cultures),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentDraftBranchOperationStatus> result = _elementDraftBranchService.Schedule(
            id,
            branchId,
            requestModel.PublishDate?.UtcDateTime,
            requestModel.Cultures);

        return result.Success
            ? Ok()
            : MapFailure(result.Result);
    }
}
