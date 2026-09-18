using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides a factory for creating presentation models for collections of content items.
/// This factory is generic and supports various content, collection response, value response, and variant response types.
/// </summary>
public abstract class ContentCollectionPresentationFactory<TContent, TCollectionResponseModel, TValueResponseModelBase, TVariantResponseModel>
    where TContent : class, IContentBase
    where TCollectionResponseModel : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    private readonly FlagProviderCollection _flagProviderCollection;
    private readonly IUmbracoMapper _mapper;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentCollectionPresentationFactory{TContent, TCollectionResponseModel, TValueResponseModelBase, TVariantResponseModel}"/> class.
    /// </summary>
    /// <param name="mapper">The mapper used to map content items to collection response models.</param>
    /// <param name="flagProviderCollection">The collection of flag providers used to populate flags on the response models.</param>
    /// <param name="userService">The service used to resolve the names of the creating and updating users.</param>
    protected ContentCollectionPresentationFactory(
        IUmbracoMapper mapper,
        FlagProviderCollection flagProviderCollection,
        IUserService userService)
    {
        _mapper = mapper;
        _flagProviderCollection = flagProviderCollection;
        _userService = userService;
    }

    /// <summary>
    /// Asynchronously creates a list of collection response models from the specified paged content collection.
    /// </summary>
    /// <param name="contentCollection">The paged model containing the content items and configuration used to generate the collection response models.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of <typeparamref name="TCollectionResponseModel"/> instances representing the collection items.</returns>
    public async Task<List<TCollectionResponseModel>> CreateCollectionModelAsync(ListViewPagedModel<TContent> contentCollection)
    {
        PagedModel<TContent> collectionItemsResult = contentCollection.Items;
        ListViewConfiguration collectionConfiguration = contentCollection.ListViewConfiguration;

        var collectionPropertyAliases = collectionConfiguration
            .IncludeProperties
            .Select(p => p.Alias)
            .WhereNotNull()
            .ToArray();

        // Pre-resolve all unique creator/writer user names in a single batch call.
        Dictionary<int, string?> userNameDictionary = ResolveUserNames(collectionItemsResult.Items);

        List<TCollectionResponseModel> collectionResponseModels =
            _mapper.MapEnumerable<TContent, TCollectionResponseModel>(collectionItemsResult.Items, context =>
            {
                context.SetIncludedProperties(collectionPropertyAliases);
                context.SetUserNameDictionary(userNameDictionary);
            });

        await SetUnmappedProperties(contentCollection, collectionResponseModels);

        PopulateHasChildren(collectionResponseModels);

        await PopulateFlags(collectionResponseModels);

        return collectionResponseModels;
    }

    /// <summary>
    /// Gets the navigation structure for the collection's item type, used to resolve whether each item has children.
    /// </summary>
    /// <remarks>
    /// When <c>null</c>, items are left reporting no children.
    /// </remarks>
    protected virtual INavigationQueryService? NavigationQueryService => null;

    /// <summary>
    /// Gets the recycle bin navigation structure for the collection's item type. Consulted for items that are
    /// absent from the main structure, so a collection viewed under a trashed ancestor still resolves correctly.
    /// </summary>
    protected virtual IRecycleBinNavigationQueryService? RecycleBinNavigationQueryService => null;

    /// <summary>
    /// Sets any properties on the collection response models that are not automatically mapped from the content items.
    /// </summary>
    /// <param name="contentCollection">The paged model containing the content items and configuration used to generate the collection response models.</param>
    /// <param name="collectionResponseModels">The list of collection response models to set unmapped properties on.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task SetUnmappedProperties(ListViewPagedModel<TContent> contentCollection, List<TCollectionResponseModel> collectionResponseModels) => Task.CompletedTask;

    private void PopulateHasChildren(List<TCollectionResponseModel> models)
    {
        INavigationQueryService? navigationQueryService = NavigationQueryService;
        IRecycleBinNavigationQueryService? recycleBinNavigationQueryService = RecycleBinNavigationQueryService;
        if (navigationQueryService is null || recycleBinNavigationQueryService is null)
        {
            return;
        }

        foreach (TCollectionResponseModel model in models)
        {
            if (model is IHasChildren target)
            {
                target.HasChildren = HasChildren(navigationQueryService, recycleBinNavigationQueryService, model.Id);
            }
        }
    }

    private static bool HasChildren(
        INavigationQueryService navigationQueryService,
        IRecycleBinNavigationQueryService recycleBinNavigationQueryService,
        Guid key)
    {
        if (navigationQueryService.TryGetHasChildren(key, out var hasChildren))
        {
            return hasChildren;
        }

        // Trashed items live in the recycle bin structure rather than the main one.
        return recycleBinNavigationQueryService.TryGetHasChildrenInBin(key, out var hasChildrenInBin)
               && hasChildrenInBin;
    }

    private async Task PopulateFlags(IEnumerable<TCollectionResponseModel> models)
    {
        foreach (IFlagProvider signProvider in _flagProviderCollection.Where(x => x.CanProvideFlags<TCollectionResponseModel>()))
        {
            await signProvider.PopulateFlagsAsync(models);
        }
    }

    private Dictionary<int, string?> ResolveUserNames(IEnumerable<TContent> items)
    {
        var uniqueUserIds = new HashSet<int>();
        foreach (TContent item in items)
        {
            uniqueUserIds.Add(item.CreatorId);
            uniqueUserIds.Add(item.WriterId);
        }

        // Filter out the default 0 ID (unset CreatorId/WriterId from TreeEntityBase) that won't
        // resolve to a user. Seed it as null so CommonMapper won't fall back to per-item GetProfileById.
        var result = _userService
            .GetUsersById(uniqueUserIds.Where(id => id != 0).ToArray())
            .ToDictionary(u => u.Id, u => u.Name);

        if (uniqueUserIds.Contains(0))
        {
            result[0] = null;
        }

        return result;
    }
}
