using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal class DocumentTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory
{
    public ContentTypeCreateModel MapCreateModel(CreateDocumentTypeRequestModel requestModel)
    {
        ContentTypeCreateModel createModel = MapContentTypeEditingModel<
            ContentTypeCreateModel,
            ContentTypePropertyTypeModel,
            ContentTypePropertyContainerModel,
            CreateDocumentTypePropertyTypeRequestModel,
            CreateDocumentTypePropertyTypeContainerRequestModel
        >(requestModel);

        // TODO: fill in the blanks

        return createModel;
    }

    public ContentTypeUpdateModel MapUpdateModel(UpdateDocumentTypeRequestModel requestModel)
    {
        ContentTypeUpdateModel updateModel = MapContentTypeEditingModel<
            ContentTypeUpdateModel,
            ContentTypePropertyTypeModel,
            ContentTypePropertyContainerModel,
            UpdateDocumentTypePropertyTypeRequestModel,
            UpdateDocumentTypePropertyTypeContainerRequestModel
        >(requestModel);

        // TODO: fill in the blanks

        return updateModel;
    }
}
