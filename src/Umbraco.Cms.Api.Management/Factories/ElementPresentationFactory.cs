using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

// TODO ELEMENTS: lots of code here was duplicated from DocumentPresentationFactory - abstract and refactor
/// <summary>
/// Factory for creating element presentation models from domain models.
/// </summary>
public class ElementPresentationFactory : IElementPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The mapper used to map between Umbraco models.</param>
    /// <param name="idKeyMap">Service for mapping between IDs and keys.</param>
    public ElementPresentationFactory(IUmbracoMapper umbracoMapper, IIdKeyMap idKeyMap)
    {
        _umbracoMapper = umbracoMapper;
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc />
    public ElementResponseModel CreateResponseModel(IElement element, ContentScheduleCollection schedule)
    {
        ElementResponseModel responseModel = _umbracoMapper.Map<ElementResponseModel>(element)!;
        _umbracoMapper.Map(schedule, responseModel);

        return responseModel;
    }

    /// <inheritdoc />
    public ElementItemResponseModel CreateItemResponseModel(IElementEntitySlim entity)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(entity.ParentId, UmbracoObjectTypes.ElementContainer);

        var responseModel = new ElementItemResponseModel
        {
            Id = entity.Key,
            Parent = parentKeyAttempt.Success ? new ReferenceByIdModel { Id = parentKeyAttempt.Result } : null,
            HasChildren = entity.HasChildren,
        };

        responseModel.DocumentType = _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!;

        responseModel.Variants = CreateVariantsItemResponseModels(entity);

        return responseModel;
    }

    /// <inheritdoc />
    public IEnumerable<ElementVariantItemResponseModel> CreateVariantsItemResponseModels(IElementEntitySlim entity)
    {
        if (entity.Variations.VariesByCulture() is false)
        {
            var model = new ElementVariantItemResponseModel()
            {
                Name = entity.Name ?? string.Empty,
                State = DocumentVariantStateHelper.GetState(entity, null),
                Culture = null,
            };

            yield return model;
            yield break;
        }

        foreach (KeyValuePair<string, string> cultureNamePair in entity.CultureNames)
        {
            var model = new ElementVariantItemResponseModel()
            {
                Name = cultureNamePair.Value,
                Culture = cultureNamePair.Key,
                State = DocumentVariantStateHelper.GetState(entity, cultureNamePair.Key)
            };

            yield return model;
        }
    }

    /// <inheritdoc />
    public DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IElementEntitySlim entity)
        => _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!;
}
