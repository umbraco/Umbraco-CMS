using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides editing services for content items.
/// </summary>
public interface IContentEditingService
{
    /// <summary>
    ///     Gets a content item by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the content item.</param>
    /// <returns>The content item, or <c>null</c> if not found.</returns>
    Task<IContent?> GetAsync(Guid key);

    /// <summary>
    ///     Validates a content creation model without persisting it.
    /// </summary>
    /// <param name="createModel">The model containing the content data to validate.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the validation result or an error status.</returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ContentCreateModel createModel, Guid userKey);

    /// <summary>
    ///     Validates a content update model without persisting it.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to validate.</param>
    /// <param name="updateModel">The model containing the content data to validate.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the validation result or an error status.</returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateContentUpdateModel updateModel, Guid userKey);

    /// <summary>
    ///     Creates a new content item.
    /// </summary>
    /// <param name="createModel">The model containing the content data.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the creation result or an error status.</returns>
    Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, Guid userKey);

    /// <summary>
    ///     Creates and publishes a new content item.
    /// </summary>
    /// <param name="createModel">The model containing the content data.</param>
    /// <param name="culturesToPublish">The cultures to publish.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the creation result or an error status.</returns>
    // TODO (V19): Remove default implementation.
    Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAndPublishAsync(ContentCreateModel createModel, string[] culturesToPublish, Guid userKey)
        => throw new NotImplementedException();

    /// <summary>
    ///     Updates an existing content item.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to update.</param>
    /// <param name="updateModel">The model containing the updated content data.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the update result or an error status.</returns>
    Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentUpdateModel updateModel, Guid userKey);

    /// <summary>
    ///     Updates and publishes an existing content item.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to update.</param>
    /// <param name="updateModel">The model containing the updated content data.</param>
    /// <param name="culturesToPublish">The cultures to publish.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the update result or an error status.</returns>
    // TODO (V19): Remove default implementation.
    Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAndPublishAsync(Guid key, ContentUpdateModel updateModel, string[] culturesToPublish, Guid userKey)
        => throw new NotImplementedException();

    /// <summary>
    ///     Moves a content item to the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to move.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the moved content item or an error status.</returns>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Deletes a content item if it is in the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the deleted content item or an error status.</returns>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Moves a content item to a new parent.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to move.</param>
    /// <param name="parentKey">The unique identifier of the new parent, or <c>null</c> to move to the root.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the moved content item or an error status.</returns>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    /// <summary>
    ///     Copies a content item to a new location.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to copy.</param>
    /// <param name="parentKey">The unique identifier of the parent for the copy, or <c>null</c> for the root.</param>
    /// <param name="relateToOriginal">Whether to create a relation between the copy and the original.</param>
    /// <param name="includeDescendants">Whether to also copy all descendant content items.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the copied content item or an error status.</returns>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey);

    /// <summary>
    ///     Sorts content items under a parent.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent, or <c>null</c> for root-level sorting.</param>
    /// <param name="sortingModels">The collection of sorting models defining the new order.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>The operation status indicating success or failure.</returns>
    Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey);

    /// <summary>
    ///     Sorts the children of a parent by a system field.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent, or <c>null</c> for root-level sorting.</param>
    /// <param name="field">The system field to sort the children by.</param>
    /// <param name="direction">The direction to sort in.</param>
    /// <param name="culture">The culture whose variant name to sort by, or <c>null</c> to sort by the invariant name. Only applies when sorting by <see cref="ContentSortField.Name"/>. The culture is not validated: a child that does not vary by the given culture - or an unrecognised culture - falls back to the invariant name.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>The operation status indicating success or failure.</returns>
    Task<ContentEditingOperationStatus> SortByFieldAsync(Guid? parentKey, ContentSortField field, Direction direction, string? culture, Guid userKey)
        => throw new NotImplementedException(); // TODO (V19): Remove default implementation.

    /// <summary>
    ///     Deletes a content item whether it is in the recycle bin or not.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the deleted content item or an error status.</returns>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Restores a content item from the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to restore.</param>
    /// <param name="parentKey">The unique identifier of the parent to restore to, or <c>null</c> for the original location.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the restored content item or an error status.</returns>
    [Obsolete("Use the overload that takes an includeDescendants parameter instead. Scheduled for removal in Umbraco 19.")]
    Task<Attempt<IContent?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey);

    /// <summary>
    ///     Restores a content item from the recycle bin, optionally leaving its descendants behind.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to restore.</param>
    /// <param name="parentKey">The unique identifier of the parent to restore to, or <c>null</c> for the original location.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <param name="includeDescendants">
    ///     Whether to restore the descendants of the content item along with it. When <c>false</c>, only the content
    ///     item itself is restored and its descendants remain in the recycle bin as top-level bin items, ready to be
    ///     restored later.
    /// </param>
    /// <returns>An attempt containing the restored content item or an error status.</returns>
    // TODO (V19): Remove the default implementation when the obsolete overload without includeDescendants is removed.
    Task<Attempt<IContent?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey, bool includeDescendants)
    {
        // Only the whole-tree restore can be satisfied by delegating to the existing method; there is no way to honour
        // includeDescendants: false without the concrete implementation, so fail fast rather than silently restore
        // the descendants after all.
        if (includeDescendants is false)
        {
            throw new NotImplementedException("This IContentEditingService implementation does not support restoring without descendants. Override the RestoreAsync overload that takes an includeDescendants parameter to support it.");
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return RestoreAsync(key, parentKey, userKey);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
