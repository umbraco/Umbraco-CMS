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

    // SaveAndPublish(TContent, string[], int) has been retired from this interface in favour of the
    // async SaveAndPublishAsync (declared on IAsyncPublishableContentService<TContent>).

    // Unpublish(TContent, string?, int) has been retired from this interface in favour of the async
    // UnpublishAsync (declared on IAsyncPublishableContentService<TContent>).

    // Rollback(int, int, string, int) has been retired from this interface in favour of the async
    // RollbackAsync (declared on IAsyncPublishableContentService<TContent>).

    // PerformScheduledPublish(DateTime) has been retired from this interface in favour of the async
    // PerformScheduledPublishAsync (declared on IAsyncPublishableContentService<TContent>).
}
