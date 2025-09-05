using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly SignProviderCollection _signProviderCollection;
    private readonly IUmbracoMapper _mapper;

    [Obsolete("Please use the controller with all parameters, will be removed in Umbraco 18")]
    protected ContentCollectionPresentationFactory(IUmbracoMapper mapper)
        : this(
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<SignProviderCollection>())
    {
    }

    protected ContentCollectionPresentationFactory(
        IUmbracoMapper mapper,
        SignProviderCollection signProviderCollection)
    {
        _mapper = mapper;
        _signProviderCollection = signProviderCollection;
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

        List<TCollectionResponseModel> collectionResponseModels =
            _mapper.MapEnumerable<TContent, TCollectionResponseModel>(collectionItemsResult.Items, context =>
            {
                context.SetIncludedProperties(collectionPropertyAliases);
            });

        await SetUnmappedProperties(contentCollection, collectionResponseModels);


        await PopulateSigns(collectionResponseModels);

        return collectionResponseModels;
    }

    protected virtual Task SetUnmappedProperties(ListViewPagedModel<TContent> contentCollection, List<TCollectionResponseModel> collectionResponseModels) => Task.CompletedTask;

    private async Task PopulateSigns(IEnumerable<TCollectionResponseModel> models)
    {
        foreach (ISignProvider signProvider in _signProviderCollection.Where(x => x.CanProvideSigns<TCollectionResponseModel>()))
        {
            await signProvider.PopulateSignsAsync(models);
        }
    }
}
