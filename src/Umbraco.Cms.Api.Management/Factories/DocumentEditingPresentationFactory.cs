using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentEditingPresentationFactory
{
    public ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(requestModel);
        model.ContentTypeKey = requestModel.ContentTypeKey;
        model.TemplateKey = requestModel.TemplateKey;
        model.ParentKey = requestModel.ParentKey;

        return model;
    }

    public ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel)
    {
        ContentUpdateModel model = MapContentEditingModel<ContentUpdateModel>(requestModel);
        model.TemplateKey = requestModel.TemplateKey;

        return model;
    }
}
