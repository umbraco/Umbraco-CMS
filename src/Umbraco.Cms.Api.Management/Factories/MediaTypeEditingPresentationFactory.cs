using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory<IMediaType>, IMediaTypeEditingPresentationFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.MediaTypeEditingPresentationFactory"/> class,
    /// providing the required media type service dependency.
    /// </summary>
    /// <param name="mediaTypeService">The service used to manage media types.</param>
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
        createModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedMediaTypes);
        createModel.ListView = requestModel.Collection?.Id;

        IDictionary<Guid, ViewModels.ContentType.CompositionType> compositionTypesByKey = CompositionTypesByKey(requestModel.Compositions);
        createModel.Compositions = MapCompositions(compositionTypesByKey);
        createModel.ContainerKey = CalculateCreateContainerKey(requestModel.Parent, compositionTypesByKey);

        return createModel;
    }

    /// <summary>
    /// Maps the given <see cref="UpdateMediaTypeRequestModel"/> to a <see cref="MediaTypeUpdateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing the media type update data.</param>
    /// <returns>A <see cref="MediaTypeUpdateModel"/> representing the updated media type.</returns>
    public MediaTypeUpdateModel MapUpdateModel(UpdateMediaTypeRequestModel requestModel)
    {
        MediaTypeUpdateModel updateModel = MapContentTypeEditingModel<
            MediaTypeUpdateModel,
            MediaTypePropertyTypeModel,
            MediaTypePropertyContainerModel,
            UpdateMediaTypePropertyTypeRequestModel,
            UpdateMediaTypePropertyTypeContainerRequestModel
        >(requestModel);

        updateModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedMediaTypes);
        updateModel.Compositions = MapCompositions(CompositionTypesByKey(requestModel.Compositions));

        updateModel.ListView = requestModel.Collection?.Id;

        return updateModel;
    }

    /// <summary>
    /// Maps a collection of content type available compositions to a collection of available media type composition response models.
    /// </summary>
    /// <param name="compositionResults">The collection of content type available compositions to map.</param>
    /// <returns>A collection of available media type composition response models.</returns>
    public IEnumerable<AvailableMediaTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults)
        => compositionResults.Select(MapCompositionModel<AvailableMediaTypeCompositionResponseModel>);

    private IEnumerable<ContentTypeSort> MapAllowedContentTypes(IEnumerable<MediaTypeSort> allowedMediaTypes)
        => MapAllowedContentTypes(allowedMediaTypes
            .DistinctBy(t => t.MediaType.Id)
            .ToDictionary(t => t.MediaType.Id, t => t.SortOrder));

    private IDictionary<Guid, ViewModels.ContentType.CompositionType> CompositionTypesByKey(IEnumerable<MediaTypeComposition> documentTypeCompositions)
        => documentTypeCompositions
            .DistinctBy(c => c.MediaType.Id)
            .ToDictionary(c => c.MediaType.Id, c => c.CompositionType);
}
