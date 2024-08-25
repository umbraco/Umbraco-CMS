using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
public class AllowedAtRootMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllowedAtRootMediaTypeController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("allowed-at-root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedMediaType>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AllowedAtRoot(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<IMediaType> result = await _mediaTypeService.GetAllAllowedAsRootAsync(skip, take);

        List<AllowedMediaType> viewModels = _umbracoMapper.MapEnumerable<IMediaType, AllowedMediaType>(result.Items);

        var pagedViewModel = new PagedViewModel<AllowedMediaType>
        {
            Total = result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
