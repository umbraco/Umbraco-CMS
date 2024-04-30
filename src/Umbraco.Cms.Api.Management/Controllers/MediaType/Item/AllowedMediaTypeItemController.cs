using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

[ApiVersion("1.0")]
public class AllowedMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IUmbracoMapper _mapper;

    public AllowedMediaTypeItemController(IMediaTypeEditingService mediaTypeEditingService, IUmbracoMapper mapper)
    {
        _mediaTypeEditingService = mediaTypeEditingService;
        _mapper = mapper;
    }

    [HttpGet("allowed")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(CancellationToken cancellationToken, string fileExtension, int skip = 0, int take = 100)
    {
        PagedModel<IMediaType> mediaTypes = await _mediaTypeEditingService.GetMediaTypesForFileExtensionAsync(fileExtension, skip, take);

        var result = new PagedModel<MediaTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes.Items),
            Total = mediaTypes.Total
        };
        return Ok(result);
    }
}
