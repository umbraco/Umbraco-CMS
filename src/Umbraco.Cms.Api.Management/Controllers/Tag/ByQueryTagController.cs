using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tag;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tag;

/// <summary>
/// Controller responsible for managing tags by handling tag-related queries, such as searching or filtering tags.
/// </summary>
[ApiVersion("1.0")]
public class ByQueryTagController : TagControllerBase
{
    private readonly ITagService _tagService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Tag.ByQueryTagController"/> class.
    /// </summary>
    /// <param name="tagService">Service used for tag management operations.</param>
    /// <param name="mapper">The Umbraco object mapper instance.</param>
    public ByQueryTagController(ITagService tagService, IUmbracoMapper mapper)
    {
        _tagService = tagService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paged collection of tags filtered by the specified query, tag group, and culture.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete, allowing the operation to be cancelled.</param>
    /// <param name="query">An optional search string to filter tags by their value.</param>
    /// <param name="tagGroup">An optional tag group to further filter the tags.</param>
    /// <param name="culture">An optional culture identifier to filter tags by culture.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set. Used for paging.</param>
    /// <param name="take">The maximum number of items to return in the result set. Used for paging.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{TagResponseModel}"/> representing the filtered and paged tags.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<TagResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of tags.")]
    [EndpointDescription("Gets a collection of tags filtered by the provided query string.")]
    public async Task<ActionResult<PagedViewModel<TagResponseModel>>> ByQuery(
        CancellationToken cancellationToken,
        string? query,
        string? tagGroup,
        string? culture,
        int skip = 0,
        int take = 100)
    {
        IEnumerable<ITag> result = await _tagService.GetByQueryAsync(query ?? string.Empty, tagGroup, culture);

        List<TagResponseModel> responseModels = _mapper.MapEnumerable<ITag, TagResponseModel>(result);

        var pagedViewModel = new PagedViewModel<TagResponseModel>
        {
            Items = responseModels.Skip(skip).Take(take),
            Total = responseModels.Count,
        };

        return Ok(pagedViewModel);
    }
}
