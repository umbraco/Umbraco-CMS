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
public class BatchMediaTypesController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchMediaTypesController"/> class.
    /// </summary>
    /// <param name="mediaTypeService">The media type service.</param>
    /// <param name="umbracoMapper">The presentation model mapper.</param>
    public BatchMediaTypesController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<MediaTypeResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple media types.")]
    [EndpointDescription("Gets multiple media types identified by the provided Ids.")]
    public async Task<IActionResult> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];

        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<MediaTypeResponseModel>());
        }

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(requestedIds);

        List<IMediaType> ordered = OrderByRequestedIds(mediaTypes, requestedIds);

        var responseModels = ordered.Select(mt => _umbracoMapper.Map<MediaTypeResponseModel>(mt)!).ToList();

        return Ok(new BatchResponseModel<MediaTypeResponseModel>
        {
            Total = responseModels.Count,
            Items = responseModels,
        });
    }
}
