using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase{TItem}" />.
/// </summary>
/// <remarks>
///     Provides the typed lookup and batch save every asynchronous content service exposes.
///     <see cref="IContentService" /> and <see cref="IElementService" /> back it with asynchronous
///     implementations; <see cref="IMemberService" /> implements it by delegating to its synchronous members so that
///     members can share the asynchronous content editing base. Media remains on
///     <see cref="IContentServiceBase{TItem}" /> alone.
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
