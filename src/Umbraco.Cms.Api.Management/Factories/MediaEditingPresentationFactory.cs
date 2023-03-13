using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaEditingPresentationFactory : ContentEditingPresentationFactory<MediaValueModel, MediaVariantRequestModel>, IMediaEditingPresentationFactory
{
    public MediaCreateModel MapCreateModel(CreateMediaRequestModel createRequestModel)
    {
        MediaCreateModel model = MapContentEditingModel<MediaCreateModel>(createRequestModel);
        model.ContentTypeKey = createRequestModel.ContentTypeKey;
        model.ParentKey = createRequestModel.ParentKey;

        return model;
    }

    public MediaUpdateModel MapUpdateModel(UpdateMediaRequestModel updateRequestModel)
    {
        MediaUpdateModel model = MapContentEditingModel<MediaUpdateModel>(updateRequestModel);

        return model;
    }
}
