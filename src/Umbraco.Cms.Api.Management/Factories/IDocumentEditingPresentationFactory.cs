using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Patching;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines a factory for creating presentation models used in editing Umbraco documents.
/// </summary>
public interface IDocumentEditingPresentationFactory
{
    /// <summary>
    /// Maps the given <see cref="CreateDocumentRequestModel"/> to a <see cref="ContentCreateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing data to create the document.</param>
    /// <returns>A <see cref="ContentCreateModel"/> representing the document to be created.</returns>
    ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel);

    /// <summary>
    /// Maps the given <see cref="UpdateDocumentRequestModel" /> to a <see cref="ContentUpdateModel" />.
    /// </summary>
    /// <param name="requestModel">The update document request model to map from.</param>
    /// <returns>The mapped content update model.</returns>
    ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel);

    /// <summary>
    /// Creates an <see cref="UpdateDocumentRequestModel"/> from the given <see cref="IContent"/>,
    /// mapping its properties, variants, and template into the request model representation.
    /// </summary>
    /// <param name="content">The content item to create the update request model from.</param>
    /// <returns>An <see cref="UpdateDocumentRequestModel"/> representing the content.</returns>
    Task<UpdateDocumentRequestModel> CreateUpdateRequestModelAsync(IContent content);

    // TODO (V19): Remove the default implementation.
    /// <summary>
    /// Maps a <see cref="PatchDocumentRequestModel"/> to a <see cref="ContentPatchModel"/>,
    /// extracting the affected cultures and segments from the patch operation paths.
    /// </summary>
    /// <param name="requestModel">The patch document request model.</param>
    /// <returns>A <see cref="ContentPatchModel"/> containing the mapped operations and affected cultures/segments.</returns>
    ContentPatchModel MapPatchModel(PatchDocumentRequestModel requestModel) => throw new NotImplementedException();

    // TODO (V19): Remove the default implementation.
    /// <summary>
    /// Maps a <see cref="ValidateUpdateDocumentRequestModel"/> to a <see cref="ValidateContentUpdateModel"/> for update validation.
    /// </summary>
    /// <param name="requestModel">The validate update document request model.</param>
    /// <returns>A <see cref="ValidateContentUpdateModel"/> ready for validation.</returns>
    ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel) => throw new NotImplementedException();
}
