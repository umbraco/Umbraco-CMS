using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc cref="IElementPresentationFactory" />
internal sealed class ElementPresentationFactory
    : PublishableContentPresentationFactoryBase<IElementEntitySlim, ElementVariantItemResponseModel>,
      IElementPresentationFactory
{
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The mapper used to map between Umbraco models.</param>
    /// <param name="idKeyMap">Service for mapping between IDs and keys.</param>
    /// <param name="flagProviderCollection">Collection of providers for document flags.</param>
    public ElementPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IIdKeyMap idKeyMap,
        FlagProviderCollection flagProviderCollection)
        : base(umbracoMapper, flagProviderCollection) =>
        _idKeyMap = idKeyMap;

    /// <inheritdoc />
    public ElementResponseModel CreateResponseModel(IElement element, ContentScheduleCollection schedule)
    {
        ElementResponseModel responseModel = UmbracoMapper.Map<ElementResponseModel>(element)!;
        UmbracoMapper.Map(schedule, responseModel);

        return responseModel;
    }

    /// <inheritdoc />
    public async Task<ElementItemResponseModel> CreateItemResponseModelAsync(IElementEntitySlim entity)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(entity.ParentId, UmbracoObjectTypes.ElementContainer);

        var responseModel = new ElementItemResponseModel
        {
            Id = entity.Key,
            Parent = parentKeyAttempt.Success ? new ReferenceByIdModel { Id = parentKeyAttempt.Result } : null,
            HasChildren = entity.HasChildren,
            DocumentType = CreateDocumentTypeReferenceResponseModel(entity),
            Variants = await CreateVariantsItemResponseModelsAsync(entity),
        };

        await PopulateFlagsAsync(responseModel);

        return responseModel;
    }

    /// <inheritdoc />
    protected override ElementVariantItemResponseModel CreateVariantItemResponseModel(
        string name,
        DocumentVariantState state,
        string? culture)
        => new() { Name = name, State = state, Culture = culture };
}
