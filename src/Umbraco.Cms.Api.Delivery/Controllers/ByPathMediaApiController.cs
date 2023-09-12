﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class ByPathMediaApiController : MediaApiControllerBase
{
    private readonly IApiMediaQueryService _apiMediaQueryService;

    public ByPathMediaApiController(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiMediaWithCropsResponseBuilder apiMediaWithCropsResponseBuilder,
        IApiMediaQueryService apiMediaQueryService)
        : base(publishedSnapshotAccessor, apiMediaWithCropsResponseBuilder)
        => _apiMediaQueryService = apiMediaQueryService;

    /// <summary>
    ///     Gets a media item by its path.
    /// </summary>
    /// <param name="path">The path of the media item.</param>
    /// <returns>The media item or not found result.</returns>
    [HttpGet("item/{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiMediaWithCropsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByPath(string path)
    {
        path = DecodePath(path);

        IPublishedContent? media = _apiMediaQueryService.GetByPath(path);
        if (media is null)
        {
            return await Task.FromResult(NotFound());
        }

        return Ok(BuildApiMediaWithCrops(media));
    }
}
