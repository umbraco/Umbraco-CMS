using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaEditingPresentationFactory : ContentEditingPresentationFactory<MediaValueModel, MediaVariantRequestModel>, IMediaEditingPresentationFactory
{
    /// <summary>
    /// Maps a <see cref="CreateMediaRequestModel"/> to a <see cref="MediaCreateModel"/>.
    /// </summary>
    /// <param name="createRequestModel">The request model containing data to create media.</param>
    /// <returns>A <see cref="MediaCreateModel"/> representing the media to be created.</returns>

    public MediaCreateModel MapCreateModel(CreateMediaRequestModel createRequestModel)
    {
        MediaCreateModel model = MapContentEditingModel<MediaCreateModel>(createRequestModel);
        model.Key = createRequestModel.Id;
        model.ContentTypeKey = createRequestModel.MediaType.Id;
        model.ParentKey = createRequestModel.Parent?.Id;

        return model;
    }

    /// <summary>
    /// Maps an UpdateMediaRequestModel to a MediaUpdateModel.
    /// </summary>
    /// <param name="updateRequestModel">The update request model containing media update data.</param>
    /// <returns>A MediaUpdateModel representing the updated media.</returns>
    public MediaUpdateModel MapUpdateModel(UpdateMediaRequestModel updateRequestModel)
    {
        MediaUpdateModel model = MapContentEditingModel<MediaUpdateModel>(updateRequestModel);

        return model;
    }
}
