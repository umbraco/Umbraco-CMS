using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase{TItem}" />.
/// </summary>
/// <remarks>
///     Started as a pure 1:1 copy of <see cref="IContentServiceBase{TItem}" /> plus <see cref="GetByIdAsync" />.
///     <c>GetById(Guid)</c> has since been retired from this tier now that <see cref="GetByIdAsync" /> is its
///     replacement - this interface is implemented only by <see cref="IContentService" /> so far, so removing it
///     here is scoped to Document.
/// </remarks>
/// <typeparam name="TContent">The type of content item managed by this service.</typeparam>
public interface IAsyncContentServiceBase<TContent> : IAsyncContentServiceBase
    where TContent : class, IContentBase
{
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
