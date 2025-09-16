using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

[ApiVersion("1.0")]
public class ChildrenDictionaryTreeController : DictionaryTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenDictionaryTreeController(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService, dictionaryItemService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ChildrenDictionaryTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDictionaryItemService dictionaryItemService)
        : base(entityService, flagProviders, dictionaryItemService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100)
    {
        PagedModel<IDictionaryItem> paginatedItems = await DictionaryItemService.GetPagedAsync(parentId, skip, take);

        return Ok(PagedViewModel(await MapTreeItemViewModels(paginatedItems.Items), paginatedItems.Total));
    }
}
