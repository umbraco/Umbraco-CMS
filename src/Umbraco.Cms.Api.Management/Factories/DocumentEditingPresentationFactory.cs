using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentEditingPresentationFactory
{
    public ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(requestModel);
        model.Key = requestModel.Id;
        model.ContentTypeKey = requestModel.DocumentType.Id;
        model.TemplateKey = requestModel.Template?.Id;
        model.ParentKey = requestModel.Parent?.Id;

        return model;
    }

    public ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel)
        => MapUpdateContentModel<ContentUpdateModel>(requestModel);

    public ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel)
    {
        ValidateContentUpdateModel model = MapUpdateContentModel<ValidateContentUpdateModel>(requestModel);
        model.Cultures = requestModel.Cultures;

        return model;
    }

    private TUpdateModel MapUpdateContentModel<TUpdateModel>(UpdateDocumentRequestModel requestModel)
        where TUpdateModel : ContentUpdateModel, new()
    {
        TUpdateModel model = MapContentEditingModel<TUpdateModel>(requestModel);
        model.TemplateKey = requestModel.Template?.Id;

        return model;
    }
}
