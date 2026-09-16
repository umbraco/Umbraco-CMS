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

    // DeleteOfTypes(IEnumerable<int>, int) has been retired from this interface in favour of the async
    // DeleteOfTypesAsync (declared on IAsyncPublishableContentService<TContent>).

    // PersistContentSchedule(IPublishableContentBase, ContentScheduleCollection) has been retired from
    // this interface in favour of the async PersistContentScheduleAsync (declared on
    // IAsyncPublishableContentService<TContent>).

    // Publish(TContent, string[], int) has been retired from this interface in favour of the async
    // PublishAsync (declared on IAsyncPublishableContentService<TContent>).

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

    // Unpublish(TContent, string?, int) has been retired from this interface in favour of the async
    // UnpublishAsync (declared on IAsyncPublishableContentService<TContent>).

    // Rollback(int, int, string, int) has been retired from this interface in favour of the async
    // RollbackAsync (declared on IAsyncPublishableContentService<TContent>).

    /// <summary>
    ///     Publishes and unpublishes scheduled content.
    /// </summary>
    /// <param name="date">The date to use for determining scheduled actions.</param>
    /// <returns>The publish results.</returns>
    IEnumerable<PublishResult> PerformScheduledPublish(DateTime date);
}
