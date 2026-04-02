using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// Controller for managing the available composition member types that can be assigned to member types.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class AvailableCompositionMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeEditingService _memberTypeEditingService;
    private readonly IMemberTypeEditingPresentationFactory _presentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableCompositionMemberTypeController"/> class, which manages available composition member types for members.
    /// </summary>
    /// <param name="memberTypeEditingService">Service for editing member types.</param>
    /// <param name="presentationFactory">Factory for creating presentation models for member type editing.</param>
    public AvailableCompositionMemberTypeController(IMemberTypeEditingService memberTypeEditingService, IMemberTypeEditingPresentationFactory presentationFactory)
    {
        _memberTypeEditingService = memberTypeEditingService;
        _presentationFactory = presentationFactory;
    }

    /// <summary>
    /// Retrieves a collection of member types that can be used as compositions for a given member type.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="compositionModel">The request model containing the target member type ID and its current compositions and properties.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="AvailableMemberTypeCompositionResponseModel"/> representing the available member type compositions.</returns>
    [HttpPost("available-compositions")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<AvailableMemberTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets available compositions.")]
    [EndpointDescription("Gets a collection of member types that are available to use as compositions for the specified member type.")]
    public async Task<IActionResult> AvailableCompositions(
        CancellationToken cancellationToken,
        MemberTypeCompositionRequestModel compositionModel)
    {
        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions = await _memberTypeEditingService.GetAvailableCompositionsAsync(
            compositionModel.Id,
            compositionModel.CurrentCompositeIds,
            compositionModel.CurrentPropertyAliases);

        IEnumerable<AvailableMemberTypeCompositionResponseModel> responseModels = _presentationFactory.MapCompositionModels(availableCompositions);

        return Ok(responseModels);
    }
}
