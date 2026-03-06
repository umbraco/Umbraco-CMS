using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
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
    [ProducesResponseType(typeof(PagedModel<AllowedMediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media type items.")]
    [EndpointDescription("Gets a collection of allowed media type items for the specified file extension.")]
    public async Task<IActionResult> Item(CancellationToken cancellationToken, string fileExtension, int skip = 0, int take = 100)
    {
        PagedModel<MediaTypeFileExtensionMatchResult> matchResults = await _mediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, skip, take);

        var result = new PagedModel<AllowedMediaTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<MediaTypeFileExtensionMatchResult, AllowedMediaTypeItemResponseModel>(matchResults.Items),
            Total = matchResults.Total,
        };
        return Ok(result);
    }
}
