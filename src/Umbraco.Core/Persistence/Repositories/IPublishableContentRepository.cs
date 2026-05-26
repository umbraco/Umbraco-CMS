using System.Collections.Immutable;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the base implementation of a repository for publishable content items.
/// </summary>
public interface IPublishableContentRepository<TContent> : IContentRepository<int, TContent>,
    IReadRepository<Guid, TContent>
    where TContent : IPublishableContentBase
{
    /// <summary>
    ///     Gets publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns>
    ///     <see cref="ContentScheduleCollection" />
    /// </returns>
    ContentScheduleCollection GetContentSchedule(int contentId);

    /// <summary>
    ///     Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="schedule"></param>
    void PersistContentSchedule(IPublishableContentBase content, ContentScheduleCollection schedule);

    /// <summary>
    ///     Clears the publishing schedule for all entries having a date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The cutoff date.</param>
    void ClearSchedule(DateTime date);

    /// <summary>
    ///     Clears the publishing schedule for entries matching the specified action and having a date before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date.</param>
    /// <param name="action">The schedule action to clear.</param>
    void ClearSchedule(DateTime date, ContentScheduleAction action);

    /// <summary>
    ///     Checks whether there is content scheduled for expiration before the specified date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns><c>true</c> if there is content scheduled for expiration; otherwise, <c>false</c>.</returns>
    bool HasContentForExpiration(DateTime date);

    /// <summary>
    ///     Checks whether there is content scheduled for release before the specified date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns><c>true</c> if there is content scheduled for release; otherwise, <c>false</c>.</returns>
    bool HasContentForRelease(DateTime date);

    /// <summary>
    ///     Gets <see cref="TContent" /> objects having an expiration date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<TContent> GetContentForExpiration(DateTime date);

    /// <summary>
    ///     Gets <see cref="IContent" /> objects having a release date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<TContent> GetContentForRelease(DateTime date);

    /// <summary>
    ///     Get the count of published items
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     We require this on the repo because the IQuery{IContent} cannot supply the 'newest' parameter
    /// </remarks>
    int CountPublished(string? contentTypeAlias = null);

    /// <summary>
    ///     Checks whether the path to a content item is published.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns><c>true</c> if the path is published; otherwise, <c>false</c>.</returns>
    bool IsPathPublished(TContent? content);

    /// <summary>
    ///     Gets the content keys from the provided collection of keys that are scheduled for publishing.
    /// </summary>
    /// <param name="contentIds">The IDs of the content items.</param>
    /// <returns>
    ///     The provided collection of content keys filtered for those that are scheduled for publishing.
    /// </returns>
    IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(int[] contentIds) => ImmutableDictionary<int, IEnumerable<ContentSchedule>>.Empty;
}

