using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

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
    void PersistContentSchedule(TContent content, ContentScheduleCollection schedule);

    /// <summary>
    ///     Clears the publishing schedule for all entries having an a date before (lower than, or equal to) a specified date.
    /// </summary>
    void ClearSchedule(DateTime date);

    void ClearSchedule(DateTime date, ContentScheduleAction action);

    bool HasContentForExpiration(DateTime date);

    bool HasContentForRelease(DateTime date);

    /// <summary>
    ///     Gets <see cref="TContent" /> objects having an expiration date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(TContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<TContent> GetContentForExpiration(DateTime date);

    /// <summary>
    ///     Gets <see cref="TContent" /> objects having a release date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(TContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<TContent> GetContentForRelease(DateTime date);

    /// <summary>
    ///     Get the count of published items
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     We require this on the repo because the IQuery{TContent} cannot supply the 'newest' parameter
    /// </remarks>
    int CountPublished(string? contentTypeAlias = null);
}

