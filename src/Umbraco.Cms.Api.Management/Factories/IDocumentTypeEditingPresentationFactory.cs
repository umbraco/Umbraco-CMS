using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentTypeEditingPresentationFactory
{
    ContentTypeCreateModel MapCreateModel(CreateDocumentTypeRequestModel requestModel);

    ContentTypeUpdateModel MapUpdateModel(UpdateDocumentTypeRequestModel requestModel);

    // TODO: move to ContentTypeEditingPresentationFactory when the implementation has the correct <Type>s
    IEnumerable<AvailableDocumentTypeCompositionResponseModel> CreateCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults);
}
