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
public class ItemTemplateItemController : TemplateItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _mapper;

    public ItemTemplateItemController(IEntityService entityService, IUmbracoMapper mapper)
    {
        _entityService = entityService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<TemplateItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] SortedSet<Guid> ids)
    {
        IEnumerable<IEntitySlim> templates = _entityService.GetAll(UmbracoObjectTypes.Template, ids.ToArray());
        List<TemplateItemResponseModel> responseModels = _mapper.MapEnumerable<IEntitySlim, TemplateItemResponseModel>(templates);
        return Ok(responseModels);
    }
}
