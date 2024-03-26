using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory<IMemberType>, IMemberTypeEditingPresentationFactory
{
    public MemberTypeEditingPresentationFactory(IMemberTypeService memberTypeService)
        : base(memberTypeService)
    {
    }

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

        MapPropertyTypeSensitivityAndVisibility(createModel.Properties, requestModel.Properties);

        return createModel;
    }

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
