using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentTypeEditingPresentationFactory
{
    ContentTypeCreateModel MapCreateModel(CreateDocumentTypeRequestModel requestModel);

    ContentTypeUpdateModel MapUpdateModel(UpdateDocumentTypeRequestModel requestModel);

    IEnumerable<AvailableDocumentTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults);
}
