using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory, IMediaTypeEditingPresentationFactory
{
    public MediaTypeEditingPresentationFactory(IContentTypeService contentTypeService)
        : base(contentTypeService)
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
        createModel.ParentKey = requestModel.Folder?.Id;
        createModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedMediaTypes);
        createModel.Compositions = MapCompositions(requestModel.Compositions);

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
        updateModel.Compositions = MapCompositions(requestModel.Compositions);

        return updateModel;
    }

    private IEnumerable<ContentTypeSort> MapAllowedContentTypes(IEnumerable<MediaTypeSort> allowedMediaTypes)
        => MapAllowedContentTypes(allowedMediaTypes
            .DistinctBy(t => t.MediaType.Id)
            .ToDictionary(t => t.MediaType.Id, t => t.SortOrder));

    private IEnumerable<Composition> MapCompositions(IEnumerable<MediaTypeComposition> documentTypeCompositions)
        => MapCompositions(documentTypeCompositions
            .DistinctBy(c => c.MediaType.Id)
            .ToDictionary(c => c.MediaType.Id, c => c.CompositionType));
}
