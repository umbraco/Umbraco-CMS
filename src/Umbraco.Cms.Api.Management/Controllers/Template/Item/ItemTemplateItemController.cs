using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Item;

[ApiVersion("1.0")]
public class ItemTemplateItemController : TemplateItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly ITemplateService _templateService;

    public ItemTemplateItemController(IUmbracoMapper mapper, ITemplateService templateService)
    {
        _mapper = mapper;
        _templateService = templateService;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<TemplateItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<TemplateItemResponseModel>());
        }

        // This is far from ideal, that we pick out the entire model, however, we must do this to get the alias.
        // This is (for one) needed for when specifying master template, since alias + .cshtml
        IEnumerable<ITemplate> templates = await _templateService.GetAllAsync(ids.ToArray());
        List<TemplateItemResponseModel> responseModels = _mapper.MapEnumerable<ITemplate, TemplateItemResponseModel>(templates);
        return Ok(responseModels);
    }
}
