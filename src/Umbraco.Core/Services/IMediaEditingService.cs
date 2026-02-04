using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the Media Editing Service, which provides operations for creating, updating, deleting,
///     and managing <see cref="IMedia"/> items through the editing API.
/// </summary>
public interface IMediaEditingService
{
    /// <summary>
    ///     Gets a media item by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to retrieve.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     the <see cref="IMedia"/> item if found; otherwise, <c>null</c>.
    /// </returns>
    Task<IMedia?> GetAsync(Guid key);

    /// <summary>
    ///     Validates a media creation model without persisting it.
    /// </summary>
    /// <param name="createModel">The <see cref="MediaCreateModel"/> containing the media data to validate.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the <see cref="ContentValidationResult"/> and
    ///     <see cref="ContentEditingOperationStatus"/> indicating the validation outcome.
    /// </returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(MediaCreateModel createModel);

    /// <summary>
    ///     Validates a media update model without persisting it.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to validate the update for.</param>
    /// <param name="updateModel">The <see cref="MediaUpdateModel"/> containing the updated media data to validate.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the <see cref="ContentValidationResult"/> and
    ///     <see cref="ContentEditingOperationStatus"/> indicating the validation outcome.
    /// </returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MediaUpdateModel updateModel);

    /// <summary>
    ///     Creates a new media item.
    /// </summary>
    /// <param name="createModel">The <see cref="MediaCreateModel"/> containing the media data to create.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the <see cref="MediaCreateResult"/> containing
    ///     the created media and <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<Attempt<MediaCreateResult, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, Guid userKey);

    /// <summary>
    ///     Updates an existing media item.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to update.</param>
    /// <param name="updateModel">The <see cref="MediaUpdateModel"/> containing the updated media data.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the <see cref="MediaUpdateResult"/> containing
    ///     the updated media and <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<Attempt<MediaUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, MediaUpdateModel updateModel, Guid userKey);

    /// <summary>
    ///     Moves a media item to the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to move to the recycle bin.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the moved <see cref="IMedia"/> item (if successful)
    ///     and <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Permanently deletes a media item.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the deleted <see cref="IMedia"/> item (if successful)
    ///     and <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Moves a media item to a new parent location.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to move.</param>
    /// <param name="parentKey">The unique identifier of the new parent, or <c>null</c> to move to the root.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the moved <see cref="IMedia"/> item (if successful)
    ///     and <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    /// <summary>
    ///     Sorts media items under a specified parent.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent, or <c>null</c> for root-level sorting.</param>
    /// <param name="sortingModels">The collection of <see cref="SortingModel"/> items defining the sort order.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey);

    /// <summary>
    ///     Permanently deletes a media item from the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to delete from the recycle bin.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the deleted <see cref="IMedia"/> item (if successful)
    ///     and <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Restores a media item from the recycle bin to a specified parent location.
    /// </summary>
    /// <param name="key">The unique identifier of the media item to restore.</param>
    /// <param name="parentKey">The unique identifier of the parent to restore to, or <c>null</c> to restore to the root.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult,TStatus}"/> with the restored <see cref="IMedia"/> item (if successful)
    ///     and <see cref="ContentEditingOperationStatus"/> indicating the operation outcome.
    /// </returns>
    Task<Attempt<IMedia?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey);
}
