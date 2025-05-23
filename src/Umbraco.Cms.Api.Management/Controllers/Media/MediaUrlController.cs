using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class MediaUrlController : MediaControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IMediaUrlFactory _mediaUrlFactory;

    public MediaUrlController(
        IMediaService mediaService,
        IMediaUrlFactory mediaUrlFactory)
    {
        _mediaService = mediaService;
        _mediaUrlFactory = mediaUrlFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("urls")]
    [ProducesResponseType(typeof(IEnumerable<MediaUrlInfoResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetUrls([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IMedia> items = _mediaService.GetByIds(ids);

        return Task.FromResult<IActionResult>(Ok(_mediaUrlFactory.CreateUrlSets(items)));
    }
}
