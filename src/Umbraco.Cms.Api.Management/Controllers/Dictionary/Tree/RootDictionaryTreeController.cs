using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

[ApiVersion("1.0")]
public class RootDictionaryTreeController : DictionaryTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public RootDictionaryTreeController(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
              dictionaryItemService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public RootDictionaryTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDictionaryItemService dictionaryItemService)
        : base(entityService, flagProviders, dictionaryItemService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of dictionary items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of dictionary items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Root(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<IDictionaryItem> paginatedItems = await DictionaryItemService.GetPagedAsync(null, skip, take);

        return Ok(PagedViewModel(await MapTreeItemViewModels(paginatedItems.Items), paginatedItems.Total));
    }
}
