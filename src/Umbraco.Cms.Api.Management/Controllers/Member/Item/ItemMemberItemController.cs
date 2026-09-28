using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

/// <summary>
/// API controller responsible for managing individual member items within the Umbraco CMS.
/// Provides endpoints for retrieving, updating, or deleting member item details.
/// </summary>
[ApiVersion("1.0")]
public class ItemMemberItemController : MemberItemControllerBase
{
    private readonly IMemberPresentationService _memberPresentationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemMemberItemController"/> class.
    /// </summary>
    /// <param name="memberPresentationService">Service for resolving members across both content and external stores.</param>
    public ItemMemberItemController(IMemberPresentationService memberPresentationService)
        => _memberPresentationService = memberPresentationService;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member items.")]
    [EndpointDescription("Gets a collection of member items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<MemberItemResponseModel>());
        }

        IEnumerable<MemberItemResponseModel> responseModels = await _memberPresentationService.CreateItemResponseModelsAsync(ids);
        return Ok(responseModels);
    }
}
