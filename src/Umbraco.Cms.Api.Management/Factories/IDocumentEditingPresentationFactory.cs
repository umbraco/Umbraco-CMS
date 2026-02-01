using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentEditingPresentationFactory
{
    ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel);

    ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel);

    /// <summary>
    /// Creates an UpdateDocumentRequestModel from IContent.
    /// Converts property values using ToEditor transformation.
    /// </summary>
    /// <param name="content">The content to convert.</param>
    /// <returns>An UpdateDocumentRequestModel representing the content.</returns>
    Task<UpdateDocumentRequestModel> CreateUpdateRequestModelAsync(IContent content);

    ContentPatchModel MapPatchModel(PatchDocumentRequestModel requestModel);

    ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel);
}
