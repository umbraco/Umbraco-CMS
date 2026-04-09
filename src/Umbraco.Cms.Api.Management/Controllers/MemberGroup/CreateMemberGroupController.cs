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
/// API controller responsible for handling requests to create new member groups.
/// </summary>
[ApiVersion("1.0")]
public class CreateMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMemberGroupController"/> class, used to handle member group creation requests.
    /// </summary>
    /// <param name="memberGroupService">Service for managing member groups.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    public CreateMemberGroupController(IMemberGroupService memberGroupService, IUmbracoMapper mapper)
    {
        _memberGroupService = memberGroupService;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new member group based on the provided configuration.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The details of the member group to create.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that returns:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the member group is created successfully.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// </list>
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Creates a new member group.")]
    [EndpointDescription("Creates a new member group with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateMemberGroupRequestModel model)
    {
        IMemberGroup? memberGroup = _mapper.Map<IMemberGroup>(model);
        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await _memberGroupService.CreateAsync(memberGroup!);
        return result.Success
            ? CreatedAtId<ByKeyMemberGroupController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : MemberGroupOperationStatusResult(result.Status);
    }
}
