using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingFactory : ContentEditingFactory<DocumentValueModel, DocumentVariantModelBase>, IDocumentEditingFactory
{
    public ContentCreateModel MapCreateModel(DocumentCreateRequestModel createRequestModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(createRequestModel);
        model.ContentTypeKey = createRequestModel.ContentTypeKey;
        model.TemplateKey = createRequestModel.TemplateKey;
        model.ParentKey = createRequestModel.ParentKey;

        return model;
    }

    public ContentUpdateModel MapUpdateModel(DocumentUpdateRequestModel updateRequestModel)
    {
        ContentUpdateModel model = MapContentEditingModel<ContentUpdateModel>(updateRequestModel);
        model.TemplateKey = updateRequestModel.TemplateKey;

        return model;
    }
}
