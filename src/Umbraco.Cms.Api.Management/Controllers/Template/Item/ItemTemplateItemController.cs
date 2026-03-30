using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Item;

/// <summary>
/// Provides API endpoints for managing item templates in Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemTemplateItemController : TemplateItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly ITemplateService _templateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Template.Item.ItemTemplateItemController"/> class with the specified mapper and template service.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used for mapping objects.</param>
    /// <param name="templateService">The <see cref="ITemplateService"/> instance used for template operations.</param>
    public ItemTemplateItemController(IUmbracoMapper mapper, ITemplateService templateService)
    {
        _mapper = mapper;
        _templateService = templateService;
    }

    /// <summary>
    /// Retrieves a collection of template items corresponding to the specified IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of unique identifiers for the template items to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with the collection of matching template items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<TemplateItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of template items.")]
    [EndpointDescription("Gets a collection of template items identified by the provided Ids.")]
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
