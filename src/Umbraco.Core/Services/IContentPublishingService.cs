using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;
using static Umbraco.Cms.Core.Constants.Conventions;

namespace Umbraco.Cms.Core.Services;

public interface IContentPublishingService
{
    /// <summary>
    ///     Publishes a single content item.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultureAndSchedule">The cultures to publish and their publishing schedules.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Result of the publish operation.</returns>
    [Obsolete("Use non obsoleted version instead. Scheduled for removal in v17")]
    Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(Guid key, CultureAndScheduleModel cultureAndSchedule, Guid userKey);

    /// <summary>
    ///     Publishes a content branch.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="force">A value indicating whether to force-publish content that is not already published.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Result of the publish operation.</returns>
    [Obsolete("This method is not longer used as the 'force' parameter has been extended into options for publishing unpublished and re-publishing changed content. Please use the overload containing the parameter for those options instead. Scheduled for removal in Umbraco 17.")]
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, bool force, Guid userKey);

    /// <summary>
    ///     Publishes a content branch.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="publishBranchFilter">A value indicating options for force publishing unpublished or re-publishing unchanged content.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Result of the publish operation.</returns>
    [Obsolete("Please use the overload containing all parameters. Scheduled for removal in Umbraco 17.")]
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, PublishBranchFilter publishBranchFilter, Guid userKey)
#pragma warning disable CS0618 // Type or member is obsolete
        => PublishBranchAsync(key, cultures, publishBranchFilter.HasFlag(PublishBranchFilter.IncludeUnpublished), userKey);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    ///     Publishes a content branch.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="publishBranchFilter">A value indicating options for force publishing unpublished or re-publishing unchanged content.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <param name="useBackgroundThread">Flag indicating whether to use a background thread for the operation and immediately return to the caller.</param>
    /// <returns>Result of the publish operation.</returns>
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, PublishBranchFilter publishBranchFilter, Guid userKey, bool useBackgroundThread)
#pragma warning disable CS0618 // Type or member is obsolete
        => PublishBranchAsync(key, cultures, publishBranchFilter, userKey);
#pragma warning restore CS0618 // Type or member is obsolete

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
    /// <returns></returns>
    Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        Guid key,
        ICollection<CulturePublishScheduleModel> culturesToPublishOrSchedule,
        Guid userKey) => StaticServiceProvider.Instance.GetRequiredService<ContentPublishingService>()
        .PublishAsync(key, culturesToPublishOrSchedule, userKey);
}
