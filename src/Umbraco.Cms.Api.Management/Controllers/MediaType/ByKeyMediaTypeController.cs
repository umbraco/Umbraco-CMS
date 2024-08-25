using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
public class ByKeyMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyMediaTypeController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IMediaType? mediaType = await _mediaTypeService.GetAsync(id);
        if (mediaType == null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        MediaTypeResponseModel model = _umbracoMapper.Map<MediaTypeResponseModel>(mediaType)!;
        return await Task.FromResult(Ok(model));
    }
}
