using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IElementEditingService
{
    /// <summary>
    ///     Gets an element by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the element.</param>
    /// <returns>The element, or <c>null</c> if not found.</returns>
    Task<IElement?> GetAsync(Guid key);

    /// <summary>
    ///     Validates an element creation model without persisting it.
    /// </summary>
    /// <param name="createModel">The model containing the element data to validate.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the validation result or an error status.</returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ElementCreateModel createModel, Guid userKey);

    /// <summary>
    ///     Validates an element update model without persisting it.
    /// </summary>
    /// <param name="key">The unique identifier of the element to validate.</param>
    /// <param name="updateModel">The model containing the element data to validate.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the validation result or an error status.</returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateElementUpdateModel updateModel, Guid userKey);

    /// <summary>
    ///     Creates a new element.
    /// </summary>
    /// <param name="createModel">The model containing the element data.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the creation result or an error status.</returns>
    Task<Attempt<ElementCreateResult, ContentEditingOperationStatus>> CreateAsync(ElementCreateModel createModel, Guid userKey);

    /// <summary>
    ///     Creates an element from a block held in a content item's property.
    /// </summary>
    /// <param name="createModel">Which block to take, and where to put the element.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>An attempt containing the created element or an error status.</returns>
    /// <remarks>
    ///     The element is created as a draft: publishing it is the editor's to do, the same as for any other
    ///     element they have not published yet. Its values are read from what the owner has stored, and nothing
    ///     comes from the caller except the name and where to put it, which is why this does not go through the
    ///     ordinary create pipeline - there are no caller-supplied fields to validate or narrow.
    /// </remarks>
    Task<Attempt<IElement?, ElementCreateFromBlockOperationStatus>> CreateFromBlockAsync(
        CreateElementFromBlockModel createModel,
        Guid userKey);

    /// <summary>
    ///     Creates and publishes a new element.
    /// </summary>
    /// <param name="createModel">The model containing the element data.</param>
    /// <param name="culturesToPublish">The cultures to publish.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the creation result or an error status.</returns>
    // TODO (V19): Remove default implementation.
    Task<Attempt<ElementCreateResult, ContentEditingOperationStatus>> CreateAndPublishAsync(ElementCreateModel createModel, string[] culturesToPublish, Guid userKey)
        => throw new NotImplementedException();

    /// <summary>
    ///     Updates an existing element.
    /// </summary>
    /// <param name="key">The unique identifier of the element to update.</param>
    /// <param name="updateModel">The model containing the updated element data.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the update result or an error status.</returns>
    Task<Attempt<ElementUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ElementUpdateModel updateModel, Guid userKey);

    /// <summary>
    ///     Updates and publishes an existing element.
    /// </summary>
    /// <param name="key">The unique identifier of the element to update.</param>
    /// <param name="updateModel">The model containing the updated element data.</param>
    /// <param name="culturesToPublish">The cultures to publish.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the update result or an error status.</returns>
    // TODO (V19): Remove default implementation.
    Task<Attempt<ElementUpdateResult, ContentEditingOperationStatus>> UpdateAndPublishAsync(Guid key, ElementUpdateModel updateModel, string[] culturesToPublish, Guid userKey)
        => throw new NotImplementedException();

    /// <summary>
    ///     Deletes an element whether it is in the recycle bin or not.
    /// </summary>
    /// <param name="key">The unique identifier of the element to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the deleted element or an error status.</returns>
    Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Moves an element to a new container.
    /// </summary>
    /// <param name="key">The unique identifier of the element to move.</param>
    /// <param name="containerKey">The unique identifier of the new container, or <c>null</c> to move to the root.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the moved element or an error status.</returns>
    Task<Attempt<ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? containerKey, Guid userKey);

    /// <summary>
    ///     Copies an element to a new location.
    /// </summary>
    /// <param name="key">The unique identifier of the element to copy.</param>
    /// <param name="containerKey">The unique identifier of the container for the copy, or <c>null</c> for the root.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the copied element or an error status.</returns>
    Task<Attempt<IElement?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? containerKey, Guid userKey);

    /// <summary>
    ///     Moves an element to the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the element to move.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the moved element or an error status.</returns>
    Task<Attempt<ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Deletes an element if it is in the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the element to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the deleted element or an error status.</returns>
    Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Restores an element from the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the element to restore.</param>
    /// <param name="containerKey">The unique identifier of the container to restore to, or <c>null</c> for the original location.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the restored element or an error status.</returns>
    Task<Attempt<ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? containerKey, Guid userKey);
}
