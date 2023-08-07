using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory, IMediaTypeEditingPresentationFactory
{
    public MediaTypeCreateModel MapCreateModel(CreateMediaTypeRequestModel requestModel)
    {
        MediaTypeCreateModel createModel = MapContentTypeEditingModel<
            MediaTypeCreateModel,
            MediaTypePropertyTypeModel,
            MediaTypePropertyContainerModel,
            CreateMediaTypePropertyTypeRequestModel,
            CreateMediaTypePropertyTypeContainerRequestModel
        >(requestModel);

        createModel.Key = requestModel.Key;
        createModel.ParentKey = requestModel.ParentKey;

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
}
