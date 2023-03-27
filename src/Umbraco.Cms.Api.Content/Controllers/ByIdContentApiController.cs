using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Content.Controllers;

public class ByIdContentApiController : ContentApiControllerBase
{
    public ByIdContentApiController(IApiPublishedContentCache apiPublishedContentCache, IApiContentBuilder apiContentBuilder)
        : base(apiPublishedContentCache, apiContentBuilder)
    {
    }

    /// <summary>
    ///     Gets a content item by id.
    /// </summary>
    /// <param name="id">The unique identifier of the content item.</param>
    /// <returns>The content item or not found result.</returns>
    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ById(Guid id)
    {
        IPublishedContent? contentItem = ApiPublishedContentCache.GetById(id);

        if (contentItem is null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(ApiContentBuilder.Build(contentItem)));
    }
}
