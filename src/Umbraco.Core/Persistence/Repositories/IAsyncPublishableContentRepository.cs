using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the async repository contract for publishable content items, adding publishing and scheduling operations.
/// </summary>
public interface IAsyncPublishableContentRepository<TContent> : IAsyncContentRepository<TContent>
    where TContent : IPublishableContentBase
{
    /// <summary>
    ///     Gets the publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentKey">The Guid key of the content node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The schedule collection for the specified content node.</returns>
    Task<ContentScheduleCollection> GetContentScheduleAsync(Guid contentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Persists the publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content">The content node whose schedule is being persisted.</param>
    /// <param name="schedule">The schedule collection to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PersistContentScheduleAsync(IPublishableContentBase content, ContentScheduleCollection schedule, CancellationToken cancellationToken);

    /// <summary>
    ///     Clears all schedule entries with a date on or before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date; all entries on or before this date are cleared.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ClearScheduleAsync(DateTime date, CancellationToken cancellationToken);

    /// <summary>
    ///     Clears schedule entries matching the specified action with a date on or before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date; matching entries on or before this date are cleared.</param>
    /// <param name="action">The schedule action (<see cref="ContentScheduleAction.Release" /> or <see cref="ContentScheduleAction.Expire" />) to clear.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ClearScheduleAsync(DateTime date, ContentScheduleAction action, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a value indicating whether any content is scheduled for expiration on or before the specified date.
    /// </summary>
    /// <param name="date">The date to check against.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if there is content scheduled for expiration; otherwise, <c>false</c>.</returns>
    Task<bool> HasContentForExpirationAsync(DateTime date, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a value indicating whether any content is scheduled for release on or before the specified date.
    /// </summary>
    /// <param name="date">The date to check against.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if there is content scheduled for release; otherwise, <c>false</c>.</returns>
    Task<bool> HasContentForReleaseAsync(DateTime date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets content items with an expiration date on or before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     Content items due for expiration. Items may be culture variant; use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus" /> to get the status for a specific culture.
    /// </returns>
    Task<IEnumerable<TContent>> GetContentForExpirationAsync(DateTime date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets content items with a release date on or before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     Content items due for release. Items may be culture variant; use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus" /> to get the status for a specific culture.
    /// </returns>
    Task<IEnumerable<TContent>> GetContentForReleaseAsync(DateTime date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the count of currently published content items, optionally filtered by content type alias.
    /// </summary>
    /// <param name="contentTypeAlias">
    ///     The alias of the content type to filter by, or <c>null</c> to count all published items.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of published items matching the filter.</returns>
    Task<int> CountPublishedAsync(string? contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a value indicating whether the complete path from the root to the specified content node is published.
    /// </summary>
    /// <param name="content">The content node to check, or <c>null</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if every ancestor in the path is published; otherwise, <c>false</c>.</returns>
    Task<bool> IsPathPublishedAsync(TContent? content, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the publish/unpublish schedules for the specified content keys.
    /// </summary>
    /// <param name="contentKeys">The Guid keys of the content nodes to retrieve schedules for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A dictionary keyed by content Guid, where each value is the collection of schedules for that node.
    /// </returns>
    Task<IDictionary<Guid, IEnumerable<ContentSchedule>>> GetContentSchedulesByKeysAsync(Guid[] contentKeys, CancellationToken cancellationToken);
}
