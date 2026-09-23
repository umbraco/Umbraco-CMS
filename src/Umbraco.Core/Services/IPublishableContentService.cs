using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     The publishable-content service contract, for documents and elements.
/// </summary>
/// <remarks>
///     Every member is asynchronous. This interface began as the async counterpart to a synchronous contract of
///     the same name, growing one member at a time as each was converted; that conversion is complete, the
///     synchronous contract is gone, and this has taken its name as the single publishable-content contract.
///     It is implemented by <see cref="IContentService" /> and <see cref="IElementService" />. Only
///     <see cref="IContentService" /> has an async EF Core repository behind it so far, so
///     <see cref="Umbraco.Cms.Core.Services.ElementService" /> bridges its members onto a synchronous engine
///     until an async element repository exists. Media and members are unrelated - they implement
///     <see cref="IContentServiceBase{TItem}" />, which remains synchronous.
/// </remarks>
/// <typeparam name="TContent">The type of content item managed by this service.</typeparam>
public interface IPublishableContentService<TContent> : IAsyncContentServiceBase<TContent>
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Gets content items by their unique identifiers.
    /// </summary>
    /// <param name="keys">The unique identifiers of the content items.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content items, in the order requested.</returns>
    Task<IEnumerable<TContent>> GetByIdsAsync(IEnumerable<Guid> keys, CancellationToken cancellationToken);

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
    /// <param name="contentKey">The unique identifier of the content to load schedule for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="ContentScheduleCollection" />.</returns>
    Task<ContentScheduleCollection> GetContentScheduleByContentIdAsync(Guid contentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content">The content to persist the schedule for.</param>
    /// <param name="contentSchedule">The content schedule collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentScheduleOperationStatus>> PersistContentScheduleAsync(IPublishableContentBase content, ContentScheduleCollection contentSchedule, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes all content of given types.
    /// </summary>
    /// <param name="contentTypeKeys">The Guid keys of the content types.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     <para>All non-deleted descendants of the deleted content is moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypesAsync(IEnumerable<Guid> contentTypeKeys, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Saves a content item.
    /// </summary>
    /// <param name="content">The content item to save.</param>
    /// <param name="userKey">The key of the user performing the action.</param>
    /// <param name="contentSchedule">The content schedule to persist alongside the save, or <c>null</c> to leave the schedule unchanged.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentSaveOperationStatus>> SaveAsync(TContent content, Guid userKey, ContentScheduleCollection? contentSchedule, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a content item.
    /// </summary>
    /// <param name="content">The content item to delete.</param>
    /// <param name="userKey">The key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    /// <remarks>
    ///     <para>This method will also delete associated media files, child content and possibly associated domains.</para>
    ///     <para>This method entirely clears the content from the database.</para>
    /// </remarks>
    Task<Attempt<ContentDeleteOperationStatus>> DeleteAsync(TContent content, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Rolls content back to a prior version.
    /// </summary>
    /// <param name="key">The Guid key of the content to roll back.</param>
    /// <param name="versionId">The version id to roll back to.</param>
    /// <param name="culture">The culture to roll back, or "*" for all cultures.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentRollbackOperationStatus>> RollbackAsync(Guid key, int versionId, string culture, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Publishes content.
    /// </summary>
    /// <remarks>
    ///     <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    ///     <para>Wildcards (*) can be used as culture identifier to publish all cultures.</para>
    ///     <para>An empty array (or a wildcard) can be passed for culture invariant content.</para>
    /// </remarks>
    /// <param name="content">The content to publish.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the publish operation.</returns>
    Task<PublishResult> PublishAsync(TContent content, string[] cultures, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Unpublishes content.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unpublishes the content as a whole when <paramref name="culture" /> is "*", but it is possible to
    ///         specify a single culture to be unpublished. Depending on whether that culture is mandatory, and
    ///         other cultures remain published, the content as a whole may or may not remain published.
    ///     </para>
    ///     <para>
    ///         If the content type is variant, then culture can be either '*' or an actual culture, but neither null nor
    ///         empty. If the content type is invariant, then culture can be either '*' or null or empty.
    ///     </para>
    /// </remarks>
    /// <param name="content">The content to unpublish.</param>
    /// <param name="culture">The culture to unpublish, or "*" for all cultures.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the unpublish operation.</returns>
    Task<PublishResult> UnpublishAsync(TContent content, string? culture, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Saves and publishes content in a single scope.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For invariant content types, <paramref name="culturesToPublish" /> must be empty; the content is
    ///         saved and the invariant culture is published.
    ///     </para>
    ///     <para>
    ///         For variant content types, only the cultures listed in <paramref name="culturesToPublish" /> are
    ///         published. Wildcards (<c>"*"</c>), nulls, whitespace and duplicate entries are not accepted. Passing
    ///         an empty array saves the content without publishing any culture.
    ///     </para>
    ///     <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    ///     <para>
    ///         The save and publish run in the same scope. If publishing fails for a business reason (for example,
    ///         invalid content or an expired schedule) the save still takes effect; both are skipped only when a
    ///         saving notification handler cancels the operation.
    ///     </para>
    /// </remarks>
    /// <param name="content">The content to publish.</param>
    /// <param name="culturesToPublish">The cultures to publish, or an empty array for invariant content.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the publish operation, or a failure result if saving failed.</returns>
    Task<PublishResult> SaveAndPublishAsync(TContent content, string[] culturesToPublish, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Publishes and unpublishes scheduled content.
    /// </summary>
    /// <param name="date">The date to use for determining scheduled actions.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The publish results.</returns>
    Task<IEnumerable<PublishResult>> PerformScheduledPublishAsync(DateTime date, CancellationToken cancellationToken);

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
