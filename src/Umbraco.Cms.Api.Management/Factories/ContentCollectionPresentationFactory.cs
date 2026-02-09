using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public abstract class ContentCollectionPresentationFactory<TContent, TCollectionResponseModel, TValueResponseModelBase, TVariantResponseModel>
    where TContent : class, IContentBase
    where TCollectionResponseModel : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    private readonly FlagProviderCollection _flagProviderCollection;
    private readonly IUmbracoMapper _mapper;
    private readonly IUserService _userService;

    protected ContentCollectionPresentationFactory(
        IUmbracoMapper mapper,
        FlagProviderCollection flagProviderCollection,
        IUserService userService)
    {
        _mapper = mapper;
        _flagProviderCollection = flagProviderCollection;
        _userService = userService;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    protected ContentCollectionPresentationFactory(
        IUmbracoMapper mapper,
        FlagProviderCollection flagProviderCollection)
        : this(
            mapper,
            flagProviderCollection,
            StaticServiceProvider.Instance.GetRequiredService<IUserService>())
    {
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    protected ContentCollectionPresentationFactory(IUmbracoMapper mapper)
        : this(
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>())
    {
    }

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

        await PopulateFlags(collectionResponseModels);

        return collectionResponseModels;
    }

    protected virtual Task SetUnmappedProperties(ListViewPagedModel<TContent> contentCollection, List<TCollectionResponseModel> collectionResponseModels) => Task.CompletedTask;

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
        Dictionary<int, string?> result = _userService
            .GetUsersById(uniqueUserIds.Where(id => id != 0).ToArray())
            .ToDictionary(u => u.Id, u => u.Name);

        if (uniqueUserIds.Contains(0))
        {
            result[0] = null;
        }

        return result;
    }
}
