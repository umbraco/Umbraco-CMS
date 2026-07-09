using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Item;

[ApiVersion("1.0")]
public class AncestorsTemplateItemController : TemplateItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AncestorsTemplateItemController(
        IItemAncestorService itemAncestorService,
        ITemplateService templateService,
        IUmbracoMapper umbracoMapper)
    {
        _itemAncestorService = itemAncestorService;
        _templateService = templateService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<TemplateItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of template items.")]
    [EndpointDescription("Gets the ancestor chains for template items identified by the provided Ids.")]
    public async Task<IActionResult> Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<TemplateItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<TemplateItemResponseModel>> result = await _itemAncestorService.GetAncestorsAsync<TemplateItemResponseModel>(
            UmbracoObjectTypes.Template,
            null,
            ids,
            async ancestors =>
            {
                Guid[] ancestorKeys = ancestors.Select(a => a.Key).ToArray();
                IEnumerable<ITemplate> templates = await _templateService.GetAllAsync(ancestorKeys);
                return _umbracoMapper.MapEnumerable<ITemplate, TemplateItemResponseModel>(templates);
            });

        return Ok(result);
    }
}
