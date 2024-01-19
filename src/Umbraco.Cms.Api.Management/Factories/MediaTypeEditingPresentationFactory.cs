using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Composition;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory<IMediaType>, IMediaTypeEditingPresentationFactory
{
    public MediaTypeEditingPresentationFactory(IMediaTypeService mediaTypeService)
        : base(mediaTypeService)
    {
    }

    public MediaTypeCreateModel MapCreateModel(CreateMediaTypeRequestModel requestModel)
    {
        MediaTypeCreateModel createModel = MapContentTypeEditingModel<
            MediaTypeCreateModel,
            MediaTypePropertyTypeModel,
            MediaTypePropertyContainerModel,
            CreateMediaTypePropertyTypeRequestModel,
            CreateMediaTypePropertyTypeContainerRequestModel
        >(requestModel);

        createModel.Key = requestModel.Id;
        createModel.ContainerKey = requestModel.ContainerId;

        return createModel;
    }

    public MediaTypeUpdateModel MapUpdateModel(UpdateMediaTypeRequestModel requestModel)
        => MapContentTypeEditingModel<
            MediaTypeUpdateModel,
            MediaTypePropertyTypeModel,
            MediaTypePropertyContainerModel,
            UpdateMediaTypePropertyTypeRequestModel,
            UpdateMediaTypePropertyTypeContainerRequestModel
        >(requestModel);

    public IEnumerable<AvailableMediaTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults)
        => compositionResults.Select(MapCompositionModel<AvailableMediaTypeCompositionResponseModel>);
}
