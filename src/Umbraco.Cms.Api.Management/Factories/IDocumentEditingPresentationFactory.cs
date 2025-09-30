using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentEditingPresentationFactory
{
    ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel);

    ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel);

    ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel);
}
