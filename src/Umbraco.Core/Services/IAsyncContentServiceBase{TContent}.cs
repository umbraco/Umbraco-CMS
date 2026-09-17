using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase{TItem}" />.
/// </summary>
/// <remarks>
///     Started as a pure 1:1 copy of <see cref="IContentServiceBase{TItem}" /> plus <see cref="GetByIdAsync" />.
///     Both of the members it inherited have since been converted - <c>GetById(Guid)</c> to
///     <see cref="GetByIdAsync" />, and the plural <c>Save</c> to <see cref="SaveAsync" /> - so this tier is now
///     fully asynchronous. It is implemented by <see cref="IContentService" /> and <see cref="IElementService" />;
///     media and members remain on the synchronous <see cref="IContentServiceBase{TItem}" />.
/// </remarks>
/// <typeparam name="TContent">The type of content item managed by this service.</typeparam>
public interface IAsyncContentServiceBase<TContent> : IAsyncContentServiceBase
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Saves a collection of content items as a single batch.
    /// </summary>
    /// <param name="contents">The content items to save.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    /// <remarks>
    ///     The batch is saved under a single scope and reported through one set of collection-level
    ///     notifications, so cancelling the save cancels every item - none are persisted.
    /// </remarks>
    Task<Attempt<ContentSaveOperationStatus>> SaveAsync(IEnumerable<TContent> contents, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a content item by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the content item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content item, or <c>null</c> if not found.</returns>
    Task<TContent?> GetByIdAsync(Guid key, CancellationToken cancellationToken);
}
