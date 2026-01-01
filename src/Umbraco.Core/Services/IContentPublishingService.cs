using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentPublishingService
{
    /// <summary>
    ///     Publishes a content branch.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="publishBranchFilter">A value indicating options for force publishing unpublished or re-publishing unchanged content.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <param name="useBackgroundThread">Flag indicating whether to use a background thread for the operation and immediately return to the caller.</param>
    /// <returns>Result of the publish operation.</returns>
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, PublishBranchFilter publishBranchFilter, Guid userKey, bool useBackgroundThread);

    /// <summary>
    /// Gets the status of a background task that is publishing a content branch.
    /// </summary>
    /// <param name="taskId">The task Id.</param>
    /// <returns>True if the requested publish branch tag is still in process.</returns>
    Task<bool> IsPublishingBranchAsync(Guid taskId) => Task.FromResult(false);

    /// <summary>
    /// Retrieves the result of a background task that has published a content branch.
    /// </summary>
    /// <param name="taskId">The task Id.</param>
    /// <returns>Result of the publish operation.</returns>
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> GetPublishBranchResultAsync(Guid taskId) => Task.FromResult(Attempt.FailWithStatus(ContentPublishingOperationStatus.TaskResultNotFound, new ContentPublishingBranchResult()));

    /// <summary>
    ///     Unpublishes multiple cultures of a single content item.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to unpublish. Use null to unpublish all cultures.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Status of the publish operation.</returns>
    Task<Attempt<ContentPublishingOperationStatus>> UnpublishAsync(Guid key, ISet<string>? cultures, Guid userKey);

    /// <summary>
    /// Publishes a single content item.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="culturesToPublishOrSchedule">The cultures to publish or schedule.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Result of the publish operation.</returns>
    Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        Guid key,
        ICollection<CulturePublishScheduleModel> culturesToPublishOrSchedule,
        Guid userKey);

    /// <summary>
    /// Publishes a single content item using an already-loaded content entity.
    /// </summary>
    /// <param name="content">The content entity to publish.</param>
    /// <param name="culturesToPublishOrSchedule">The cultures to publish or schedule.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <param name="skipValidation">
    /// When true, skips property validation. Only use when the caller has already validated
    /// the content (e.g., after a successful create/update with no validation errors).
    /// </param>
    /// <returns>Result of the publish operation.</returns>
    /// <remarks>
    /// Use this overload when you already have the IContent entity (e.g., after creating or updating)
    /// to avoid an unnecessary database round-trip.
    /// </remarks>
    // TODO (18): Remove the default implementation.
    Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        IContent content,
        ICollection<CulturePublishScheduleModel> culturesToPublishOrSchedule,
        Guid userKey,
        bool skipValidation = false) => PublishAsync(content.Key, culturesToPublishOrSchedule, userKey);
}
