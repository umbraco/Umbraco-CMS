using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase{TItem}" />.
/// </summary>
/// <remarks>
///     A pure 1:1 copy of <see cref="IContentServiceBase{TItem}" /> — same members, unchanged, only the interface
///     name gets the "Async" suffix — plus <see cref="GetByIdAsync" />, the one member from this tier that has
///     already been migrated. This is scaffolding: giving the async hierarchy the exact shape of the sync one up
///     front means later increments only ever swap an implementation, never touch a contract, so migrating one
///     member at a time never risks breaking other consumers of the shared base class (e.g. <see cref="ElementService" />,
///     which still derives from the original synchronous class).
/// </remarks>
/// <typeparam name="TContent">The type of content item managed by this service.</typeparam>
public interface IAsyncContentServiceBase<TContent> : IAsyncContentServiceBase
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Gets a content item by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the content item.</param>
    /// <returns>The content item, or <c>null</c> if not found.</returns>
    TContent? GetById(Guid key);

    /// <summary>
    ///     Saves a collection of content items.
    /// </summary>
    /// <param name="contents">The content items to save.</param>
    /// <param name="userId">The identifier of the user performing the save operation.</param>
    /// <returns>An attempt containing the operation result.</returns>
    Attempt<OperationResult?> Save(IEnumerable<TContent> contents, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a content item by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the content item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content item, or <c>null</c> if not found.</returns>
    Task<TContent?> GetByIdAsync(Guid key, CancellationToken cancellationToken);
}
