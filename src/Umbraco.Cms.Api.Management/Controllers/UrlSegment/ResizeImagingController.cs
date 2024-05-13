using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UrlSegment;

[ApiVersion("1.0")]
public class ResizeImagingController : ImagingControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IThumbnailUrlFactory _thumbnailUrlFactory;

    public ResizeImagingController(IMediaService mediaService, IThumbnailUrlFactory thumbnailUrlFactory)
    {
        _mediaService = mediaService;
        _thumbnailUrlFactory = thumbnailUrlFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("resize/urls")]
    [ProducesResponseType(typeof(IEnumerable<MediaUrlInfoResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Urls([FromQuery(Name = "id")] HashSet<Guid> ids, int height = 200, int width = 200, ImageCropMode? mode = null)
    {
        IEnumerable<IMedia> items = _mediaService.GetByIds(ids);

        return await Task.FromResult(Ok(_thumbnailUrlFactory.CreateUrlSets(items, height, width, mode)));
    }
}
