using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

    /// <summary>
    /// API controller responsible for handling operations related to updating members.
    /// </summary>
[ApiVersion("1.0")]
public class UpdateMemberController : MemberControllerBase
{
    private readonly IMemberEditingService _memberEditingService;
    private readonly IMemberEditingPresentationFactory _memberEditingPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMemberController"/> class, responsible for handling member update operations in the management API.
    /// </summary>
    /// <param name="memberEditingService">Service used to perform member editing operations.</param>
    /// <param name="memberEditingPresentationFactory">Factory for creating presentation models related to member editing.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authentication.</param>
    public UpdateMemberController(
        IMemberEditingService memberEditingService,
        IMemberEditingPresentationFactory memberEditingPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberEditingService = memberEditingService;
        _memberEditingPresentationFactory = memberEditingPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a member.")]
    [EndpointDescription("Updates a member identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateMemberRequestModel updateRequestModel)
    {
        MemberUpdateModel model = _memberEditingPresentationFactory.MapUpdateModel(updateRequestModel);
        Attempt<MemberUpdateResult, MemberEditingStatus> result = await _memberEditingService.UpdateAsync(id, model, CurrentUser(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : MemberEditingStatusResult(result.Status);
    }
}
