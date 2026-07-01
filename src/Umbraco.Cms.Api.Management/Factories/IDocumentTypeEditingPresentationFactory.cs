using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating editing presentation models for document types in the management API.
/// </summary>
public interface IDocumentTypeEditingPresentationFactory
{
    /// <summary>
    /// Maps the given <see cref="CreateDocumentTypeRequestModel"/> to a <see cref="ContentTypeCreateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing data to create a document type.</param>
    /// <returns>A <see cref="ContentTypeCreateModel"/> representing the document type to be created.</returns>
    ContentTypeCreateModel MapCreateModel(CreateDocumentTypeRequestModel requestModel);

    /// <summary>
    /// Maps the given <see cref="UpdateDocumentTypeRequestModel"/> to a <see cref="ContentTypeUpdateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing the update data for the document type.</param>
    /// <returns>A <see cref="ContentTypeUpdateModel"/> representing the updated document type.</returns>
    ContentTypeUpdateModel MapUpdateModel(UpdateDocumentTypeRequestModel requestModel);

    /// <summary>
    /// Maps a collection of content type available compositions to a collection of available document type composition response models.
    /// </summary>
    /// <param name="compositionResults">The collection of content type available compositions to map.</param>
    /// <returns>An enumerable of available document type composition response models.</returns>
    IEnumerable<AvailableDocumentTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults);
}
