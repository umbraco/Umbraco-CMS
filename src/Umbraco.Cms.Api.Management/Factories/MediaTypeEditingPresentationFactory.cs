using Umbraco.Cms.Api.Management.ViewModels.MediaType;
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
        createModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedMediaTypes);
        createModel.ListView = requestModel.Collection?.Id;

        IDictionary<Guid, ViewModels.ContentType.CompositionType> compositionTypesByKey = CompositionTypesByKey(requestModel.Compositions);
        createModel.Compositions = MapCompositions(compositionTypesByKey);
        createModel.ContainerKey = CalculateCreateContainerKey(requestModel.Parent, compositionTypesByKey);

        return createModel;
    }

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
