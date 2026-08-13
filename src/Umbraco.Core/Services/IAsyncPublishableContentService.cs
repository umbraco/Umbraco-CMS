using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IPublishableContentService{TContent}" />.
/// </summary>
/// <remarks>
///     This is the async-first contract used while the content, media, and member repositories are migrated to EF
///     Core. For now it is only implemented by <see cref="IContentService" />; the media and member services continue
///     to use the synchronous <see cref="IPublishableContentService{TContent}" /> until their repositories are
///     migrated. Starts empty (all members currently live on <see cref="IAsyncContentServiceBase{TContent}" />);
///     grows one member at a time as each <see cref="IPublishableContentService{TContent}" /> member gets its async
///     conversion.
/// </remarks>
/// <typeparam name="TContent">The type of content item managed by this service.</typeparam>
public interface IAsyncPublishableContentService<TContent> : IAsyncContentServiceBase<TContent>
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Counts published content items, optionally filtered by content type alias.
    /// </summary>
    /// <param name="contentTypeAlias">The content type alias to filter by, or <c>null</c> for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of published content items matching the filter.</returns>
    Task<int> CountPublishedAsync(string? contentTypeAlias, CancellationToken cancellationToken);
}
