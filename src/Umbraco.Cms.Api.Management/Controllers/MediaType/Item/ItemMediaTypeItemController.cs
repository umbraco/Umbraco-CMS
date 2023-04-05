using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

public class ItemMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _mapper;

    public ItemMediaTypeItemController(IMediaTypeService mediaTypeService, IUmbracoMapper mapper)
    {
        _mediaTypeService = mediaTypeService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "key")] SortedSet<Guid> keys)
    {
        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetAll(keys);
        List<MediaTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes);
        return Ok(responseModels);
    }
}
