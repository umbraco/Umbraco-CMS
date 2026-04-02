using Umbraco.Cms.Api.Management.ViewModels.Document;
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
    /// Maps the given <see cref="Umbraco.Cms.Api.Management.Models.UpdateDocumentRequestModel" /> to a <see cref="Umbraco.Cms.Core.Models.ContentUpdateModel" />.
    /// </summary>
    /// <param name="requestModel">The update document request model to map from.</param>
    /// <returns>The mapped content update model.</returns>
    ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel);

    /// <summary>
    /// Maps the given <see cref="ValidateUpdateDocumentRequestModel"/> to a <see cref="ValidateContentUpdateModel"/> for validation purposes.
    /// </summary>
    /// <param name="requestModel">The request model containing the data to validate.</param>
    /// <returns>A <see cref="ValidateContentUpdateModel"/> representing the validated content update.</returns>
    ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel);
}
