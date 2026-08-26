using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

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

    /// <summary>
    ///     Saves a content item.
    /// </summary>
    /// <param name="content">The content item to save.</param>
    /// <param name="userId">The identifier of the user performing the action, or <c>null</c> to use the super user.</param>
    /// <param name="contentSchedule">The content schedule to persist alongside the save, or <c>null</c> to leave the schedule unchanged.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentSaveOperationStatus>> SaveAsync(TContent content, int? userId, ContentScheduleCollection? contentSchedule, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a version of content.
    /// </summary>
    /// <remarks>
    ///     Int-keyed rather than Guid-keyed: unlike content items, versions have no Guid key resolvable
    ///     from a caller-facing identifier today, so this takes the version's raw database id directly.
    /// </remarks>
    /// <param name="versionId">The version identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content version, or <c>null</c> if not found.</returns>
    Task<TContent?> GetVersionAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all versions of content.
    /// </summary>
    /// <param name="contentKey">The Guid key of the content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content versions.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    Task<IEnumerable<TContent>> GetVersionsAsync(Guid contentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a page of versions of content.
    /// </summary>
    /// <param name="contentKey">The Guid key of the content.</param>
    /// <param name="skip">The number of versions to skip.</param>
    /// <param name="take">The number of versions to take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content versions.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    Task<IEnumerable<TContent>> GetVersionsSlimAsync(Guid contentKey, int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a page of version ids of content.
    /// </summary>
    /// <param name="contentKey">The Guid key of the content.</param>
    /// <param name="skip">The number of versions to skip.</param>
    /// <param name="take">The maximum number of version ids to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The version ids.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    Task<IEnumerable<int>> GetVersionIdsAsync(Guid contentKey, int skip, int take, CancellationToken cancellationToken);
}
