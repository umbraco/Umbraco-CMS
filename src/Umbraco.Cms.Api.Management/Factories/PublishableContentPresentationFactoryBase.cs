using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Base class for presentation factories that create response models for publishable content entities (documents and elements).
/// </summary>
/// <typeparam name="TEntity">The publishable content entity type.</typeparam>
/// <typeparam name="TVariantItemResponseModel">The variant item response model type.</typeparam>
internal abstract class PublishableContentPresentationFactoryBase<TEntity, TVariantItemResponseModel>
    where TEntity : IPublishableContentEntitySlim
    where TVariantItemResponseModel : PublishableVariantItemResponseModelBase
{
    private readonly FlagProviderCollection _flagProviderCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishableContentPresentationFactoryBase{TEntity, TVariantItemResponseModel}"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The <see cref="IUmbracoMapper"/> instance used for mapping between models.</param>
    /// <param name="flagProviderCollection">The <see cref="FlagProviderCollection"/> instance used for determining flags to populate on response models.</param>
    protected PublishableContentPresentationFactoryBase(
        IUmbracoMapper umbracoMapper,
        FlagProviderCollection flagProviderCollection)
    {
        UmbracoMapper = umbracoMapper;
        _flagProviderCollection = flagProviderCollection;
    }

    /// <summary>
    /// Gets the <see cref="IUmbracoMapper"/> instance.
    /// </summary>
    protected IUmbracoMapper UmbracoMapper { get; }

    /// <summary>
    /// Creates variant item response models for an entity, including one per culture for culture-varying content.
    /// </summary>
    /// <param name="entity">The entity to create variant models for.</param>
    /// <returns>The variant item response models.</returns>
    [Obsolete("Use CreateVariantsItemResponseModelsAsync instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<TVariantItemResponseModel> CreateVariantsItemResponseModels(TEntity entity)
        => CreateVariantsItemResponseModelsAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously creates variant item response models for an entity, including one per culture for culture-varying content.
    /// </summary>
    /// <param name="entity">The entity to create variant models for.</param>
    /// <returns>The variant item response models.</returns>
    public async Task<IEnumerable<TVariantItemResponseModel>> CreateVariantsItemResponseModelsAsync(TEntity entity)
    {
        var models = new List<TVariantItemResponseModel>();

        if (entity.Variations.VariesByCulture() is false)
        {
            TVariantItemResponseModel model = CreateVariantItemResponseModel(entity.Name ?? string.Empty, DocumentVariantStateHelper.GetState(entity, null), null);

            await PopulateFlagsAsync(model);
            models.Add(model);
            return models;
        }

        foreach (KeyValuePair<string, string> cultureNamePair in entity.CultureNames)
        {
            TVariantItemResponseModel model = CreateVariantItemResponseModel(cultureNamePair.Value, DocumentVariantStateHelper.GetState(entity, cultureNamePair.Key), cultureNamePair.Key);

            await PopulateFlagsAsync(model);
            models.Add(model);
        }

        return models;
    }

    /// <summary>
    /// Creates a <see cref="DocumentTypeReferenceResponseModel"/> for the given entity.
    /// </summary>
    /// <param name="entity">The entity to create the document type reference for.</param>
    /// <returns>The document type reference response model.</returns>
    public DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(TEntity entity)
        => UmbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!;

    /// <summary>
    /// Creates a new variant item response model instance with the specified values.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <param name="state">The variant state.</param>
    /// <param name="culture">The culture, or <c>null</c> for invariant content.</param>
    /// <returns>A new variant item response model.</returns>
    protected abstract TVariantItemResponseModel CreateVariantItemResponseModel(
        string name,
        DocumentVariantState state,
        string? culture);

    /// <summary>
    /// Populates flags on a model using all applicable flag providers.
    /// </summary>
    /// <typeparam name="TItem">The type of the model supporting flags.</typeparam>
    /// <param name="model">The model to populate flags on.</param>
    [Obsolete("Use PopulateFlagsAsync instead. Scheduled for removal in Umbraco 19.")]
    protected void PopulateFlags<TItem>(TItem model)
        where TItem : IHasFlags
        => PopulateFlagsAsync(model).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously populates flags on a model using all applicable flag providers.
    /// </summary>
    /// <typeparam name="TItem">The type of the model supporting flags.</typeparam>
    /// <param name="model">The model to populate flags on.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task PopulateFlagsAsync<TItem>(TItem model)
        where TItem : IHasFlags
    {
        foreach (IFlagProvider flagProvider in _flagProviderCollection.Where(x => x.CanProvideFlags<TItem>()))
        {
            await flagProvider.PopulateFlagsAsync([model]);
        }
    }
}
