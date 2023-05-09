using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
public class AllStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllStylesheetController(
        IStylesheetService stylesheetService,
        IUmbracoMapper umbracoMapper)
    {
        _stylesheetService = stylesheetService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<StylesheetOverviewResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> All(int skip = 0, int take = 100)
    {
        IStylesheet[] stylesheets = (await _stylesheetService.GetAllAsync()).ToArray();

        List<StylesheetOverviewResponseModel> viewModels = _umbracoMapper.MapEnumerable<IStylesheet, StylesheetOverviewResponseModel>(stylesheets.Skip(skip).Take(take));

        return Ok(new PagedViewModel<StylesheetOverviewResponseModel> { Items = viewModels, Total = stylesheets.Length });
    }
}
