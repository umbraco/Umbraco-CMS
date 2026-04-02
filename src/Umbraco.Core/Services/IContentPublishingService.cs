using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides methods for publishing and unpublishing content.
/// </summary>
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
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> containing the result of the publish operation.</returns>
    Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        Guid key,
        ICollection<CulturePublishScheduleModel> culturesToPublishOrSchedule,
        Guid userKey) => StaticServiceProvider.Instance.GetRequiredService<ContentPublishingService>()
        .PublishAsync(key, culturesToPublishOrSchedule, userKey);
}
