using System.Diagnostics;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ByIdContentApiController : ContentApiItemControllerBase
{
    private readonly IRequestMemberAccessService _requestMemberAccessService;

    public ByIdContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IRequestMemberAccessService requestMemberAccessService)
        : base(apiPublishedContentCache, apiContentResponseBuilder)
        => _requestMemberAccessService = requestMemberAccessService;

    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public async Task<IActionResult> ById(Guid id)
        => await HandleRequest(id);

    /// <summary>
    ///     Gets a content item by id.
    /// </summary>
    /// <param name="id">The unique identifier of the content item.</param>
    /// <returns>The content item or not found result.</returns>
    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByIdV20(Guid id)
        => await HandleRequest(id);

    private async Task<IActionResult> HandleRequest(Guid id)
    {
        IPublishedContent? contentItem = await ApiPublishedContentCache.GetByIdAsync(id).ConfigureAwait(false);

        if (contentItem is null)
        {
            return NotFound();
        }
        IActionResult? deniedAccessResult = await HandleMemberAccessAsync(contentItem, _requestMemberAccessService).ConfigureAwait(false);
        if (deniedAccessResult is not null)
        {
            return deniedAccessResult;
        }
        IApiContentResponse? apiContentResponse = ApiContentResponseBuilder.Build(contentItem);
        if (apiContentResponse is null)
        {
            return NotFound();
        }

        return Ok(apiContentResponse);
    }
}
