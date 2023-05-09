using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

[ApiVersion("1.0")]
public class ByPathPartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _mapper;

    public ByPathPartialViewController(
        IPartialViewService partialViewService,
        IUmbracoMapper mapper)
    {
        _partialViewService = partialViewService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByPath(string path)
    {
        IPartialView? partialView = await _partialViewService.GetAsync(path);

        return partialView is null
            ? NotFound()
            : Ok(_mapper.Map<PartialViewResponseModel>(partialView));
    }
}
