using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IElementPublishingService
{
    /// <summary>
    /// Publishes an element.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <param name="culturesToPublishOrSchedule">The cultures to publish or schedule.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns></returns>
    Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        Guid key,
        ICollection<CulturePublishScheduleModel> culturesToPublishOrSchedule,
        Guid userKey);

    /// <summary>
    ///     Unpublishes multiple cultures of an element.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <param name="cultures">The cultures to unpublish. Use null to unpublish all cultures.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Status of the publish operation.</returns>
    Task<Attempt<ContentPublishingOperationStatus>> UnpublishAsync(Guid key, ISet<string>? cultures, Guid userKey);
}
