using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public abstract class ContentCollectionPresentationFactory<TContent, TCollectionResponseModel, TValueResponseModelBase, TVariantResponseModel>
    where TContent : class, IContentBase
    where TCollectionResponseModel : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    private readonly IUmbracoMapper _mapper;

    protected ContentCollectionPresentationFactory(IUmbracoMapper mapper) => _mapper = mapper;

    public async Task<List<TCollectionResponseModel>> CreateCollectionModelAsync(ListViewPagedModel<TContent> contentCollection)
    {
        PagedModel<TContent> collectionItemsResult = contentCollection.Items;
        ListViewConfiguration collectionConfiguration = contentCollection.ListViewConfiguration;

        var collectionPropertyAliases = collectionConfiguration
            .IncludeProperties
            .Select(p => p.Alias)
            .WhereNotNull()
            .ToArray();

        List<TCollectionResponseModel> collectionResponseModels =
            _mapper.MapEnumerable<TContent, TCollectionResponseModel>(collectionItemsResult.Items, context =>
            {
                context.SetIncludedProperties(collectionPropertyAliases);
            });

        await SetUnmappedProperties(collectionResponseModels);

        return collectionResponseModels;
    }

    protected virtual Task SetUnmappedProperties(List<TCollectionResponseModel> collectionResponseModels) => Task.CompletedTask;
}
