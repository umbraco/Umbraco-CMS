using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tag;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tag;

[ApiVersion("1.0")]
public class ByQueryTagController : TagControllerBase
{
    private readonly ITagService _tagService;
    private readonly IUmbracoMapper _mapper;

    public ByQueryTagController(ITagService tagService, IUmbracoMapper mapper)
    {
        _tagService = tagService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<TagResponseModel>), StatusCodes.Status200OK)]
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

        return await Task.FromResult(Ok(pagedViewModel));
    }
}
