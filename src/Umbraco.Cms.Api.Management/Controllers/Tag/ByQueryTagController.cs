using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tag;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tag;

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
    public async Task<ActionResult<PagedViewModel<TagResponseModel>>> ByQuery(string? tagGroup, string? culture, string? query = null, int skip = 0, int take = 100)
    {
        IEnumerable<ITag> result;

        if (query is not null)
        {
            result = await _tagService.GetByQueryAsync(query, tagGroup, culture);
        }
        else
        {
            result = await _tagService.GetAllAsync(tagGroup, culture);
        }

        List<TagResponseModel> responseModels = _mapper.MapEnumerable<ITag, TagResponseModel>(result);

        var pagedViewModel = new PagedViewModel<TagResponseModel>
        {
            Items = responseModels.Skip(skip).Take(take),
            Total = responseModels.Count,
        };

        return await Task.FromResult(Ok(pagedViewModel));
    }
}
