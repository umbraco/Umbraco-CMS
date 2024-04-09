using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaEditingPresentationFactory : ContentEditingPresentationFactory<MediaValueModel, MediaVariantRequestModel>, IMediaEditingPresentationFactory
{
    public MediaCreateModel MapCreateModel(CreateMediaRequestModel createRequestModel)
    {
        MediaCreateModel model = MapContentEditingModel<MediaCreateModel>(createRequestModel);
        model.Key = createRequestModel.Id;
        model.ContentTypeKey = createRequestModel.MediaType.Id;
        model.ParentKey = createRequestModel.Parent?.Id;

        return model;
    }

    public MediaUpdateModel MapUpdateModel(UpdateMediaRequestModel updateRequestModel)
    {
        MediaUpdateModel model = MapContentEditingModel<MediaUpdateModel>(updateRequestModel);

        return model;
    }
}
