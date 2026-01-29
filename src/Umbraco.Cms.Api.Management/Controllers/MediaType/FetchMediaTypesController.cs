using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// Provides an API controller for retrieving the full details for multiple media types by key.
/// </summary>
[ApiVersion("1.0")]
public class FetchMediaTypesController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FetchMediaTypesController"/> class.
    /// </summary>
    /// <param name="mediaTypeService">The media type service.</param>
    /// <param name="umbracoMapper">The presentation model mapper.</param>
    public FetchMediaTypesController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost("fetch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FetchResponseModel<MediaTypeResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Fetch(CancellationToken cancellationToken, FetchRequestModel requestModel)
    {
        Guid[] ids = [.. requestModel.Ids.Select(x => x.Id).Distinct()];

        if (ids.Length == 0)
        {
            return Ok(new FetchResponseModel<MediaTypeResponseModel>());
        }

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(ids);

        List<IMediaType> ordered = OrderByRequestedIds(mediaTypes, ids);

        var responseModels = ordered.Select(mt => _umbracoMapper.Map<MediaTypeResponseModel>(mt)!).ToList();

        return Ok(new FetchResponseModel<MediaTypeResponseModel>
        {
            Total = responseModels.Count,
            Items = responseModels,
        });
    }
}
