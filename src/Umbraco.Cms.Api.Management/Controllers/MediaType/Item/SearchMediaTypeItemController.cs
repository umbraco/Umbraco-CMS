using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

[ApiVersion("1.0")]
public class SearchMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _mapper;

    public SearchMediaTypeItemController(IEntitySearchService entitySearchService, IMediaTypeService mediaTypeService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _mediaTypeService = mediaTypeService;
        _mapper = mapper;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.MediaType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Task.FromResult<IActionResult>(Ok(new PagedModel<MediaTypeItemResponseModel> { Total = searchResult.Total }));
        }

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(searchResult.Items.Select(item => item.Key).ToArray().EmptyNull());
        var result = new PagedModel<MediaTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes),
            Total = searchResult.Total
        };

        return Task.FromResult<IActionResult>(Ok(result));
    }
}
