using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Controllers.Media;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ByPathMediaApiController : MediaApiControllerBase
{
    private readonly IApiMediaQueryService _apiMediaQueryService;

    public ByPathMediaApiController(
        IPublishedMediaCache publishedMediaCache,
        IApiMediaWithCropsResponseBuilder apiMediaWithCropsResponseBuilder,
        IApiMediaQueryService apiMediaQueryService)
        : base(publishedMediaCache, apiMediaWithCropsResponseBuilder)
        => _apiMediaQueryService = apiMediaQueryService;

    [HttpGet("item/{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiMediaWithCropsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public Task<IActionResult> ByPath(string path)
        => Task.FromResult(HandleRequest(path));

    /// <summary>
    ///     Gets a media item by its path.
    /// </summary>
    /// <param name="path">The path of the media item.</param>
    /// <returns>The media item or not found result.</returns>
    [HttpGet("item/{*path}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IApiMediaWithCropsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ByPathV20(string path)
        => Task.FromResult(HandleRequest(path));

    private IActionResult HandleRequest(string path)
    {
        path = DecodePath(path);

        IPublishedContent? media = _apiMediaQueryService.GetByPath(path);
        if (media is null)
        {
            return NotFound();
        }

        return Ok(BuildApiMediaWithCrops(media));
    }
}
