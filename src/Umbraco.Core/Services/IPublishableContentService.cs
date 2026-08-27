using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IPublishableContentService<TContent> : IContentServiceBase
    where TContent : class, IPublishableContentBase
{
    // Deliberately not IContentServiceBase<TContent> - that would bring back GetById(Guid), which has been
    // retired from the Document surface in favour of the async GetByIdAsync. The plural Save is redeclared
    // directly here since it's still needed by every implementer (Document, Element) and has no async
    // equivalent yet.
    /// <summary>
    ///     Saves content.
    /// </summary>
    /// <param name="contents">The content to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation result.</returns>
    Attempt<OperationResult?> Save(IEnumerable<TContent> contents, int userId = Constants.Security.SuperUserId);

    // GetByIds(IEnumerable<Guid>) has been retired from this interface in favour of the async
    // GetByIdsAsync (declared on IAsyncPublishableContentService<TContent>).

    // Save(TContent, ...) has been retired from this interface in favour of the async
    // SaveAsync (declared on IAsyncPublishableContentService<TContent>).

    /// <summary>
    ///     Deletes all content of given types.
    /// </summary>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <remarks>
    ///     <para>All non-deleted descendants of the deleted content is moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content">The content to persist the schedule for.</param>
    /// <param name="contentSchedule">The content schedule collection.</param>
    void PersistContentSchedule(IPublishableContentBase content, ContentScheduleCollection contentSchedule);

    /// <summary>
    ///     Publishes content
    /// </summary>
    /// <remarks>
    ///     <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    ///     <para>Wildcards (*) can be used as culture identifier to publish all cultures.</para>
    ///     <para>An empty array (or a wildcard) can be passed for culture invariant content.</para>
    /// </remarks>
    /// <param name="content">The content to publish.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    PublishResult Publish(TContent content, string[] cultures, int userId = Constants.Security.SuperUserId);

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
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The result of the publish operation, or a failure result if saving failed.</returns>
    PublishResult SaveAndPublish(TContent content, string[] culturesToPublish, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Unpublishes content.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, unpublishes the content as a whole, but it is possible to specify a culture to be
    ///         unpublished. Depending on whether that culture is mandatory, and other cultures remain published,
    ///         the content as a whole may or may not remain published.
    ///     </para>
    ///     <para>
    ///         If the content type is variant, then culture can be either '*' or an actual culture, but neither null nor
    ///         empty. If the content type is invariant, then culture can be either '*' or null or empty.
    ///     </para>
    /// </remarks>
    PublishResult Unpublish(TContent content, string? culture = "*", int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Rolls back the content to a specific version.
    /// </summary>
    /// <param name="id">The id of the content node.</param>
    /// <param name="versionId">The version id to roll back to.</param>
    /// <param name="culture">An optional culture to roll back.</param>
    /// <param name="userId">The identifier of the user who is performing the roll back.</param>
    /// <remarks>
    ///     <para>When no culture is specified, all cultures are rolled back.</para>
    /// </remarks>
    OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Publishes and unpublishes scheduled content.
    /// </summary>
    /// <param name="date">The date to use for determining scheduled actions.</param>
    /// <returns>The publish results.</returns>
    IEnumerable<PublishResult> PerformScheduledPublish(DateTime date);
}
