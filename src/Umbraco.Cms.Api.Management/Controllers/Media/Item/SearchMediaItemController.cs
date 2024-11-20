﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

[ApiVersion("1.0")]
public class SearchMediaItemController : MediaItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    public SearchMediaItemController(IIndexedEntitySearchService indexedEntitySearchService, IMediaPresentationFactory mediaPresentationFactory)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _mediaPresentationFactory = mediaPresentationFactory;
    }

    [NonAction]
    [Obsolete("Scheduled to be removed in v16, use the non obsoleted method instead")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
        => await SearchFromParent(cancellationToken, query, skip, take, null);

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchFromParent(CancellationToken cancellationToken, string query, int skip = 0, int take = 100, Guid? parentId = null)
    {
        PagedModel<IEntitySlim> searchResult = _indexedEntitySearchService.Search(UmbracoObjectTypes.Media, query, parentId, skip, take);
        var result = new PagedModel<MediaItemResponseModel>
        {
            Items = searchResult.Items.OfType<IMediaEntitySlim>().Select(_mediaPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total,
        };

        return await Task.FromResult(Ok(result));
    }
}
