using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models used for editing media types.
/// </summary>
public interface IMediaTypeEditingPresentationFactory
{
    /// <summary>
    /// Maps the given <see cref="CreateMediaTypeRequestModel"/> to a <see cref="MediaTypeCreateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing data to create a media type.</param>
    /// <returns>A <see cref="MediaTypeCreateModel"/> representing the created media type.</returns>
    MediaTypeCreateModel MapCreateModel(CreateMediaTypeRequestModel requestModel);

    /// <summary>
    /// Maps the given <see cref="Umbraco.Cms.Api.Management.Models.UpdateMediaTypeRequestModel"/> to a <see cref="Umbraco.Cms.Api.Management.Models.MediaTypeUpdateModel"/>.
    /// </summary>
    /// <param name="requestModel">The update request model containing media type data.</param>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.MediaTypeUpdateModel"/> representing the updated media type.</returns>
    MediaTypeUpdateModel MapUpdateModel(UpdateMediaTypeRequestModel requestModel);

    /// <summary>
    /// Maps a collection of content type composition results to their corresponding media type composition response models.
    /// </summary>
    /// <param name="compositionResults">The collection of content type composition results to map.</param>
    /// <returns>An enumerable of mapped media type composition response models.</returns>
    IEnumerable<AvailableMediaTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults);
}
