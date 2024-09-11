using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentBlueprintEditingPresentationFactory
{
    ContentBlueprintCreateModel MapCreateModel(CreateDocumentBlueprintRequestModel requestModel);

    ContentBlueprintUpdateModel MapUpdateModel(UpdateDocumentBlueprintRequestModel requestModel);
}
