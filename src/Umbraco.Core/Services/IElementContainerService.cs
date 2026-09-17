using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides methods for managing <see cref="EntityContainer"/> objects for elements, including recycle bin operations.
/// </summary>
public interface IElementContainerService : IEntityTypeContainerService<IElement>
{
    /// <summary>
    /// Moves a container, and everything below it, to a new parent container.
    /// </summary>
    /// <param name="key">The key of the container to move.</param>
    /// <param name="parentKey">The key of the parent container to move to, or null to move to the tree root.</param>
    /// <param name="userKey">Key of the user issuing the move.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> describing the outcome of the operation.</returns>
    /// <remarks>
    /// This member predates <see cref="IEntityTypeContainerService{TTreeEntity}.MoveAsync"/> and hides the inherited member,
    /// so that removing it does not break packages compiled against 18.x.
    /// </remarks>
    // TODO (V19): Remove this member and the remarks above, and let IElementContainerService inherit MoveAsync
    // from the base interface.
    new Task<Attempt<EntityContainerOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    /// <summary>
    /// Moves a container, and everything below it, to the recycle bin.
    /// </summary>
    /// <param name="key">The key of the container to move to the recycle bin.</param>
    /// <param name="userKey">Key of the user issuing the move.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> describing the outcome of the operation.</returns>
    Task<Attempt<EntityContainerOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    /// Permanently deletes a container that is in the recycle bin, along with all of its descendants.
    /// </summary>
    /// <param name="key">The key of the container to delete.</param>
    /// <param name="userKey">Key of the user issuing the deletion.</param>
    /// <returns>
    /// An <see cref="Attempt{TResult,TStatus}"/> containing the deleted container if successful, or an error status if not.
    /// </returns>
    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    /// Permanently deletes everything in the element recycle bin.
    /// </summary>
    /// <param name="userKey">Key of the user issuing the deletion.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> describing the outcome of the operation.</returns>
    Task<Attempt<EntityContainerOperationStatus>> EmptyRecycleBinAsync(Guid userKey);

    /// <summary>
    /// Restores a container, and everything below it, from the recycle bin.
    /// </summary>
    /// <param name="key">The key of the container to restore.</param>
    /// <param name="parentKey">The key of the parent container to restore to, or null to restore to the tree root.</param>
    /// <param name="userKey">Key of the user issuing the restore.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> describing the outcome of the operation.</returns>
    Task<Attempt<EntityContainerOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey);
}
