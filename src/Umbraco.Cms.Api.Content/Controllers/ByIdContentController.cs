using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Controllers;

public class ByIdContentController : ContentApiControllerBase
{
    private readonly IApiContentBuilder _apiContentBuilder;

    public ByIdContentController(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiContentBuilder apiContentBuilder)
        : base(publishedSnapshotAccessor)
        => _apiContentBuilder = apiContentBuilder;

    /// <summary>
    ///     Gets a content item by id.
    /// </summary>
    /// <param name="id">The unique identifier of the content item.</param>
    /// <returns>The content item or not found result.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ById(Guid id)
    {
        IPublishedContentCache? contentCache = GetContentCache();

        if (contentCache is null)
        {
            return await Task.FromResult(BadRequest(ContentCacheNotFoundProblemDetails()));
        }

        IPublishedContent? contentItem = contentCache.GetById(id);

        if (contentItem is null)
        {
            return await Task.FromResult(NotFound());
        }

        return await Task.FromResult(Ok(_apiContentBuilder.Build(contentItem)));
    }
}
