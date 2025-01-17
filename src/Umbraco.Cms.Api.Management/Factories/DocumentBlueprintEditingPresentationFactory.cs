using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentBlueprintEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentBlueprintEditingPresentationFactory
{
    public ContentBlueprintCreateModel MapCreateModel(CreateDocumentBlueprintRequestModel requestModel)
    {
        ContentBlueprintCreateModel model = MapContentEditingModel<ContentBlueprintCreateModel>(requestModel);
        model.Key = requestModel.Id;
        model.ContentTypeKey = requestModel.DocumentType.Id;
        model.ParentKey = requestModel.Parent?.Id;

        return model;
    }

    public ContentBlueprintUpdateModel MapUpdateModel(UpdateDocumentBlueprintRequestModel requestModel)
    {
        ContentBlueprintUpdateModel model = MapContentEditingModel<ContentBlueprintUpdateModel>(requestModel);
        return model;
    }
}
