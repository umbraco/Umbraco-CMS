using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentBlueprintEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentBlueprintEditingPresentationFactory
{
    public ContentBlueprintUpdateModel MapUpdateModel(UpdateDocumentBlueprintRequestModel requestModel)
    {
        ContentBlueprintUpdateModel model = MapContentEditingModel<ContentBlueprintUpdateModel>(requestModel);
        return model;
    }
}
