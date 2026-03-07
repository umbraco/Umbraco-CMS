using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

    /// <summary>
    /// Controller responsible for managing member entities identified by their unique key.
    /// Provides endpoints for operations on a specific member.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyMemberController : MemberControllerBase
{
    private readonly IMemberEditingService _memberEditingService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyMemberController"/> class, which handles member management operations by member key.
    /// </summary>
    /// <param name="memberEditingService">Service used to perform editing operations on members.</param>
    /// <param name="memberPresentationFactory">Factory for creating member presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public ByKeyMemberController(
        IMemberEditingService memberEditingService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberEditingService = memberEditingService;
        _memberPresentationFactory = memberPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Retrieves a member by their unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the member to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the member data if found; otherwise, a not found result.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a member.")]
    [EndpointDescription("Gets a member identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IMember? member = await _memberEditingService.GetAsync(id);
        if (member == null)
        {
            return MemberNotFound();
        }

        MemberResponseModel model = await _memberPresentationFactory.CreateResponseModelAsync(member, CurrentUser(_backOfficeSecurityAccessor));
        return Ok(model);
    }
}
