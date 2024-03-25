using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentBlueprintEditingPresentationFactory
{
    ContentBlueprintUpdateModel MapUpdateModel(UpdateDocumentBlueprintRequestModel requestModel);
}
