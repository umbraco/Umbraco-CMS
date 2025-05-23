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

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class AvailableCompositionMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeEditingService _memberTypeEditingService;
    private readonly IMemberTypeEditingPresentationFactory _presentationFactory;

    public AvailableCompositionMemberTypeController(IMemberTypeEditingService memberTypeEditingService, IMemberTypeEditingPresentationFactory presentationFactory)
    {
        _memberTypeEditingService = memberTypeEditingService;
        _presentationFactory = presentationFactory;
    }

    [HttpPost("available-compositions")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<AvailableMemberTypeCompositionResponseModel>), StatusCodes.Status200OK)]
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
