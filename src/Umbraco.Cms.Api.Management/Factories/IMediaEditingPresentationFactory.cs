using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models used in media editing operations.
/// </summary>
public interface IMediaEditingPresentationFactory
{
    /// <summary>
    /// Maps a <see cref="CreateMediaRequestModel"/> to a <see cref="MediaCreateModel"/>.
    /// </summary>
    /// <param name="createRequestModel">The create media request model to map from.</param>
    /// <returns>The mapped <see cref="MediaCreateModel"/>.</returns>
    MediaCreateModel MapCreateModel(CreateMediaRequestModel createRequestModel);

    /// <summary>
    /// Maps an <see cref="UpdateMediaRequestModel"/> to a <see cref="MediaUpdateModel"/>.
    /// </summary>
    /// <param name="updateRequestModel">The model containing the updated media information.</param>
    /// <returns>A <see cref="MediaUpdateModel"/> representing the mapped update.</returns>
    MediaUpdateModel MapUpdateModel(UpdateMediaRequestModel updateRequestModel);
}
