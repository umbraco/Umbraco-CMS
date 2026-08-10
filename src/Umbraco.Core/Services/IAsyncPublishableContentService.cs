using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentServiceBase{TItem}" />.
/// </summary>
/// <remarks>
///     This is the async-first contract used while the content, media, and member repositories are migrated to EF
///     Core. For now it is only implemented by <see cref="IContentService" />; the media and member services continue
///     to use the synchronous <see cref="IContentServiceBase{TItem}" /> until their repositories are migrated.
/// </remarks>
/// <typeparam name="TContent">The type of content item managed by this service.</typeparam>
public interface IAsyncPublishableContentService<TContent> : IService
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Gets a content item by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the content item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content item, or <c>null</c> if not found.</returns>
    Task<TContent?> GetByIdAsync(Guid key, CancellationToken cancellationToken);
}
