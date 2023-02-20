using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingFactory : ContentEditingFactory<DocumentValueModel, DocumentVariantModel>, IDocumentEditingFactory
{
    public ContentCreateModel MapCreateModel(DocumentCreateModel createModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(createModel);
        model.ContentTypeKey = createModel.ContentTypeKey;
        model.TemplateKey = createModel.TemplateKey;
        model.ParentKey = createModel.ParentKey;

        return model;
    }

    public ContentUpdateModel MapUpdateModel(DocumentUpdateModel updateModel)
    {
        ContentUpdateModel model = MapContentEditingModel<ContentUpdateModel>(updateModel);
        model.TemplateKey = updateModel.TemplateKey;

        return model;
    }
}
