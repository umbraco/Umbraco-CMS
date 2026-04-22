using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMemberPresentationService _memberPresentationService;

    // TODO (V19): Remove the unnecessary parameters provided to the constructor.

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyMemberController"/> class.
    /// </summary>
    /// <param name="memberEditingService">Service used to perform editing operations on members.</param>
    /// <param name="memberPresentationFactory">Factory for creating member presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="memberPresentationService">Service for resolving members across both content and external stores.</param>
    [ActivatorUtilitiesConstructor]
    public ByKeyMemberController(
        IMemberEditingService memberEditingService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberPresentationService memberPresentationService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberPresentationService = memberPresentationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyMemberController"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ByKeyMemberController(
        IMemberEditingService memberEditingService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            memberEditingService,
            memberPresentationFactory,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IMemberPresentationService>())
    {
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
        MemberResponseModel? model = await _memberPresentationService.CreateResponseModelByKeyAsync(id, CurrentUser(_backOfficeSecurityAccessor));
        return model is not null ? Ok(model) : MemberNotFound();
    }
}
