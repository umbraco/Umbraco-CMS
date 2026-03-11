using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Patching;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentEditingPresentationFactory
{
    /// <summary>
    /// Maps a <see cref="CreateDocumentRequestModel"/> to a <see cref="ContentCreateModel"/> for content creation.
    /// </summary>
    /// <param name="requestModel">The create document request model.</param>
    /// <returns>A <see cref="ContentCreateModel"/> ready for content creation.</returns>
    ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel);

    /// <summary>
    /// Maps an <see cref="UpdateDocumentRequestModel"/> to a <see cref="ContentUpdateModel"/> for content updating.
    /// </summary>
    /// <param name="requestModel">The update document request model.</param>
    /// <returns>A <see cref="ContentUpdateModel"/> ready for content updating.</returns>
    ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel);

    /// <summary>
    /// Creates an UpdateDocumentRequestModel from IContent.
    /// Converts property values using ToEditor transformation.
    /// </summary>
    /// <param name="content">The content to convert.</param>
    /// <returns>An UpdateDocumentRequestModel representing the content.</returns>
    Task<UpdateDocumentRequestModel> CreateUpdateRequestModelAsync(IContent content);

    /// <summary>
    /// Maps a <see cref="PatchDocumentRequestModel"/> to a <see cref="ContentPatchModel"/>,
    /// extracting the affected cultures and segments from the patch operation paths.
    /// </summary>
    /// <param name="requestModel">The patch document request model.</param>
    /// <returns>A <see cref="ContentPatchModel"/> containing the mapped operations and affected cultures/segments.</returns>
    ContentPatchModel MapPatchModel(PatchDocumentRequestModel requestModel);

    /// <summary>
    /// Maps a <see cref="ValidateUpdateDocumentRequestModel"/> to a <see cref="ValidateContentUpdateModel"/> for update validation.
    /// </summary>
    /// <param name="requestModel">The validate update document request model.</param>
    /// <returns>A <see cref="ValidateContentUpdateModel"/> ready for validation.</returns>
    ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel);
}
