using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

/// <summary>
/// Serves as the base controller for operations related to dictionary tree structures in the Umbraco CMS Management API.
/// Provides common functionality for derived controllers managing dictionary items in a hierarchical format.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDictionaryOrTemplates)]
// NOTE: at the moment dictionary items (renamed to dictionary tree) aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class DictionaryTreeControllerBase : NamedEntityTreeControllerBase<NamedEntityTreeItemResponseModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service used for managing and retrieving entities within the Umbraco system.</param>
    /// <param name="dictionaryItemService">Service used for managing and retrieving dictionary items for localization.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public DictionaryTreeControllerBase(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
              dictionaryItemService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing entities within the system.</param>
    /// <param name="flagProviders">A collection of providers for entity flags.</param>
    /// <param name="dictionaryItemService">Service for managing dictionary items.</param>
    public DictionaryTreeControllerBase(
        IEntityService entityService, 
        FlagProviderCollection flagProviders, 
        IDictionaryItemService dictionaryItemService)
        : base(entityService, flagProviders) =>
        DictionaryItemService = dictionaryItemService;

    // dictionary items do not currently have a known UmbracoObjectType, so we'll settle with Unknown for now
    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Unknown;

    protected IDictionaryItemService DictionaryItemService { get; }

    protected async Task<IEnumerable<NamedEntityTreeItemResponseModel>> MapTreeItemViewModels(IEnumerable<IDictionaryItem> dictionaryItems)
        => await MapTreeItemViewModelsAsync(dictionaryItems).ToArrayAsync();

    protected override async Task<ActionResult<IEnumerable<NamedEntityTreeItemResponseModel>>> GetAncestors(Guid descendantKey, bool includeSelf = true)
    {
        IDictionaryItem? dictionaryItem = await DictionaryItemService.GetAsync(descendantKey);
        if (dictionaryItem is null)
        {
            // this looks weird - but we actually mimic how the rest of the ancestor (and children) endpoints actually work
            return Ok(Enumerable.Empty<NamedEntityTreeItemResponseModel>());
        }

        var ancestors = new List<IDictionaryItem>();
        if (includeSelf)
        {
            ancestors.Add(dictionaryItem);
        }

        while (dictionaryItem?.ParentId is not null)
        {
            dictionaryItem = await DictionaryItemService.GetAsync(dictionaryItem.ParentId.Value);
            if (dictionaryItem is not null)
            {
                ancestors.Add(dictionaryItem);
            }
        }

        NamedEntityTreeItemResponseModel[] viewModels = await MapTreeItemViewModelsAsync(ancestors).ToArrayAsync();

        return Ok(viewModels.Reverse());
    }

    private async IAsyncEnumerable<NamedEntityTreeItemResponseModel> MapTreeItemViewModelsAsync(IEnumerable<IDictionaryItem> dictionaryItems)
    {
        foreach (IDictionaryItem dictionaryItem in dictionaryItems)
        {
            yield return await CreateEntityTreeItemViewModelAsync(dictionaryItem);
        }
    }

    private async Task<NamedEntityTreeItemResponseModel> CreateEntityTreeItemViewModelAsync(IDictionaryItem dictionaryItem)
    {
        var hasChildren = await DictionaryItemService.CountChildrenAsync(dictionaryItem.Key) > 0;
        return new NamedEntityTreeItemResponseModel
        {
            Name = dictionaryItem.ItemKey,
            Id = dictionaryItem.Key,
            HasChildren = hasChildren,
            Parent = dictionaryItem.ParentId.HasValue
                ? new ReferenceByIdModel
                {
                    Id = dictionaryItem.ParentId.Value
                }
                : null
        };
    }
}
