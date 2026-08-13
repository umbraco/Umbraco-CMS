using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IPublishableContentService{TContent}" />.
/// </summary>
/// <remarks>
///     This is the async-first contract used while the content, media, and member repositories are migrated to EF
///     Core. It's implemented by both <see cref="IContentService" /> and <see cref="IElementService" />; only
///     <see cref="IContentService" /> has an async EF Core repository behind it so far, so
///     <see cref="Umbraco.Cms.Core.Services.ElementService" /> bridges each member added here onto its existing
///     synchronous implementation until an async element repository exists. The media and member services continue
///     to use the synchronous <see cref="IPublishableContentService{TContent}" /> exclusively. Starts empty (all
///     members currently live on <see cref="IAsyncContentServiceBase{TContent}" />); grows one member at a time as
///     each <see cref="IPublishableContentService{TContent}" /> member gets its async conversion.
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

    /// <summary>
    ///     Gets content items by their unique identifiers.
    /// </summary>
    /// <param name="ids">The unique identifiers of the content items.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content items, in the order requested.</returns>
    Task<IEnumerable<TContent>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a dictionary of content keys and their matching content schedules.
    /// </summary>
    /// <param name="keys">The content keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary with a content key and an IEnumerable of matching ContentSchedules.</returns>
    Task<IDictionary<Guid, IEnumerable<ContentSchedule>>> GetContentSchedulesByKeysAsync(Guid[] keys, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentId">The unique identifier of the content to load schedule for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="ContentScheduleCollection" />.</returns>
    Task<ContentScheduleCollection> GetContentScheduleByContentIdAsync(Guid contentId, CancellationToken cancellationToken);
}
