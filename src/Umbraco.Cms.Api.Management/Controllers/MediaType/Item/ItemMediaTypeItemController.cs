using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

[ApiVersion("1.0")]
public class ItemMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _mapper;

    public ItemMediaTypeItemController(IMediaTypeService mediaTypeService, IUmbracoMapper mapper)
    {
        _mediaTypeService = mediaTypeService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<MediaTypeItemResponseModel>()));
        }

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(ids);
        List<MediaTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
