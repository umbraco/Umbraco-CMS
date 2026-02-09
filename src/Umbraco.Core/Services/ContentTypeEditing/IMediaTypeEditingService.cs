using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Service interface for creating and managing media types in the Umbraco backoffice.
/// </summary>
/// <remarks>
///     This service provides operations for creating, updating, and querying media types,
///     including support for compositions and file extension filtering.
/// </remarks>
public interface IMediaTypeEditingService
{
    /// <summary>
    ///     Creates a new media type based on the provided model.
    /// </summary>
    /// <param name="model">The model containing the media type definition including properties and compositions.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the created <see cref="IMediaType"/> on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the reason for failure.
    /// </returns>
    Task<Attempt<IMediaType?, ContentTypeOperationStatus>> CreateAsync(MediaTypeCreateModel model, Guid userKey);

    /// <summary>
    ///     Updates an existing media type with the provided model data.
    /// </summary>
    /// <param name="mediaType">The existing media type to update.</param>
    /// <param name="model">The model containing the updated media type definition.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the updated <see cref="IMediaType"/> on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the reason for failure.
    /// </returns>
    Task<Attempt<IMediaType?, ContentTypeOperationStatus>> UpdateAsync(IMediaType mediaType, MediaTypeUpdateModel model, Guid userKey);

    /// <summary>
    ///     Gets the available compositions for a media type.
    /// </summary>
    /// <param name="key">The unique identifier of the media type, or <c>null</c> for a new media type.</param>
    /// <param name="currentCompositeKeys">The keys of currently selected compositions.</param>
    /// <param name="currentPropertyAliases">The aliases of properties currently defined on the media type.</param>
    /// <returns>
    ///     A collection of <see cref="ContentTypeAvailableCompositionsResult"/> indicating which compositions
    ///     are available and which are not allowed due to conflicts.
    /// </returns>
    Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases);

    /// <summary>
    ///     Gets media types that support a specific file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to filter by (with or without the leading period).</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to return for pagination.</param>
    /// <returns>
    ///     A <see cref="PagedModel{T}"/> containing media types that can handle the specified file extension.
    /// </returns>
    /// <remarks>
    ///     This method checks media types with an <c>umbracoFile</c> property and filters based on
    ///     the configured allowed file extensions in the file upload data type configuration.
    /// </remarks>
    Task<PagedModel<IMediaType>> GetMediaTypesForFileExtensionAsync(string fileExtension, int skip, int take);

    /// <summary>
    ///     Gets media types that are considered folder types.
    /// </summary>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to return for pagination.</param>
    /// <returns>
    ///     A <see cref="PagedModel{T}"/> containing media types that function as folders.
    /// </returns>
    /// <remarks>
    ///     A media type is considered a folder type if it does not have an <c>umbracoFile</c> property
    ///     and has allowed child types configured.
    /// </remarks>
    Task<PagedModel<IMediaType>> GetFolderMediaTypes(int skip, int take);
}
