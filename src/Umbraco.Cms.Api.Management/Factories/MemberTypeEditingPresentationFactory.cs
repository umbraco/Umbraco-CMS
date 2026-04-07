using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory<IMemberType>, IMemberTypeEditingPresentationFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.MemberTypeEditingPresentationFactory"/> class,
    /// providing functionality for creating member type editing presentations.
    /// </summary>
    /// <param name="memberTypeService">The service used to manage and retrieve member types.</param>
    public MemberTypeEditingPresentationFactory(IMemberTypeService memberTypeService)
        : base(memberTypeService)
    {
    }

    /// <summary>
    /// Maps the data from a <see cref="CreateMemberTypeRequestModel"/> to a new <see cref="MemberTypeCreateModel"/> instance.
    /// This includes mapping basic properties, compositions, container keys, and property type sensitivity and visibility.
    /// </summary>
    /// <param name="requestModel">The request model containing the data required to create a member type.</param>
    /// <returns>
    /// A <see cref="MemberTypeCreateModel"/> populated with values from the <paramref name="requestModel"/>,
    /// including mapped compositions and property settings.
    /// </returns>
    public MemberTypeCreateModel MapCreateModel(CreateMemberTypeRequestModel requestModel)
    {
        MemberTypeCreateModel createModel = MapContentTypeEditingModel<
            MemberTypeCreateModel,
            MemberTypePropertyTypeModel,
            MemberTypePropertyContainerModel,
            CreateMemberTypePropertyTypeRequestModel,
            CreateMemberTypePropertyTypeContainerRequestModel
        >(requestModel);

        createModel.Key = requestModel.Id;
        createModel.Compositions = MapCompositions(requestModel.Compositions);
        createModel.ContainerKey = requestModel.Parent?.Id;

        MapPropertyTypeSensitivityAndVisibility(createModel.Properties, requestModel.Properties);

        return createModel;
    }

    /// <summary>
    /// Maps an <see cref="Umbraco.Cms.Api.Management.Models.UpdateMemberTypeRequestModel" /> to a <see cref="Umbraco.Cms.Api.Management.Models.MemberTypeUpdateModel" />.
    /// This includes mapping compositions and updating property sensitivity and visibility based on the request model.
    /// </summary>
    /// <param name="requestModel">The request model containing the updated member type information.</param>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.MemberTypeUpdateModel" /> that reflects the changes specified in the request model.</returns>
    public MemberTypeUpdateModel MapUpdateModel(UpdateMemberTypeRequestModel requestModel)
    {
        MemberTypeUpdateModel updateModel = MapContentTypeEditingModel<
            MemberTypeUpdateModel,
            MemberTypePropertyTypeModel,
            MemberTypePropertyContainerModel,
            UpdateMemberTypePropertyTypeRequestModel,
            UpdateMemberTypePropertyTypeContainerRequestModel
        >(requestModel);

        updateModel.Compositions = MapCompositions(requestModel.Compositions);

        MapPropertyTypeSensitivityAndVisibility(updateModel.Properties, requestModel.Properties);

        return updateModel;
    }

    /// <summary>
    /// Maps a collection of <see cref="ContentTypeAvailableCompositionsResult"/> objects to their corresponding <see cref="AvailableMemberTypeCompositionResponseModel"/> representations.
    /// </summary>
    /// <param name="compositionResults">The collection of composition results to map.</param>
    /// <returns>An enumerable of mapped <see cref="AvailableMemberTypeCompositionResponseModel"/> objects.</returns>
    public IEnumerable<AvailableMemberTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults)
        => compositionResults.Select(MapCompositionModel<AvailableMemberTypeCompositionResponseModel>);

    private IEnumerable<Composition> MapCompositions(IEnumerable<MemberTypeComposition> documentTypeCompositions)
        => MapCompositions(documentTypeCompositions
            .DistinctBy(c => c.MemberType.Id)
            .ToDictionary(c => c.MemberType.Id, c => c.CompositionType));

    private void MapPropertyTypeSensitivityAndVisibility<TRequestPropertyTypeModel>(
        IEnumerable<MemberTypePropertyTypeModel> propertyTypes,
        IEnumerable<TRequestPropertyTypeModel> requestPropertyTypes)
        where TRequestPropertyTypeModel : MemberTypePropertyTypeModelBase
    {
        var requestModelPropertiesByAlias = requestPropertyTypes.ToDictionary(p => p.Alias);
        foreach (MemberTypePropertyTypeModel propertyType in propertyTypes)
        {
            if (requestModelPropertiesByAlias.TryGetValue(propertyType.Alias, out TRequestPropertyTypeModel? requestPropertyType) is false)
            {
                throw new InvalidOperationException($"Could not find the property type model {propertyType.Alias} in the request");
            }

            propertyType.IsSensitive = requestPropertyType.IsSensitive;
            propertyType.MemberCanView = requestPropertyType.Visibility.MemberCanView;
            propertyType.MemberCanEdit = requestPropertyType.Visibility.MemberCanEdit;
        }
    }
}
