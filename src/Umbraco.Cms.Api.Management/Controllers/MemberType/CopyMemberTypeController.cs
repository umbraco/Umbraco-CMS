using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// API controller responsible for handling requests to copy member types in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class CopyMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyMemberTypeController"/> class with the specified member type service.
    /// </summary>
    /// <param name="memberTypeService">The service used to manage member types.</param>
    public CopyMemberTypeController(IMemberTypeService memberTypeService)
        => _memberTypeService = memberTypeService;

    /// <summary>
    /// Creates a copy of the specified member type.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member type to copy.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Obsolete("Please use the overload that includes all parameters. Scheduled for removal in Umbraco 19.")]
    [NonAction]
    public async Task<IActionResult> Copy(
        CancellationToken cancellationToken,
        Guid id) => await Copy(cancellationToken, id, null);

    /// <summary>
    /// Copies a member type with the specified ID, optionally using the provided copy options.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member type to copy.</param>
    /// <param name="copyMemberTypeRequestModel">Optional model containing copy options.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the copy operation.</returns>
    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Copies a member type.")]
    [EndpointDescription("Creates a duplicate of an existing member type identified by the provided Id.")]
    public async Task<IActionResult> Copy(
        CancellationToken cancellationToken,
        Guid id,
        CopyMemberTypeRequestModel? copyMemberTypeRequestModel)
    {
        Attempt<IMemberType?, ContentTypeStructureOperationStatus> result = await _memberTypeService.CopyAsync(id, copyMemberTypeRequestModel?.Target?.Id);

        return result.Success
            ? CreatedAtId<ByKeyMemberTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : StructureOperationStatusResult(result.Status);
    }
}
