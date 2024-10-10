using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Item;

[ApiVersion("1.0")]
public class SearchTemplateItemController : TemplateItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _mapper;

    public SearchTemplateItemController(IEntitySearchService entitySearchService, ITemplateService templateService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _templateService = templateService;
        _mapper = mapper;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<TemplateItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.Template, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<TemplateItemResponseModel> { Total = searchResult.Total });
        }

        IEnumerable<ITemplate> templates = await _templateService.GetAllAsync(searchResult.Items.Select(item => item.Key).ToArray());
        var result = new PagedModel<TemplateItemResponseModel>
        {
            Items = _mapper.MapEnumerable<ITemplate, TemplateItemResponseModel>(templates),
            Total = searchResult.Total
        };

        return Ok(result);
    }
}
