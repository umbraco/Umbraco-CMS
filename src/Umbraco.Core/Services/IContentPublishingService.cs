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
    [Obsolete("This method is not longer used as the 'force' parameter has been extended into options for publishing unpublished and re-publishing changed content. Please use the overload containing the parameter for those options instead. Will be removed in V17.")]
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, bool force, Guid userKey);

    /// <summary>
    ///     Publishes a content branch.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="forceOptions">A value indicating options for force publishing unpublished or re-publishing unchanged content.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Result of the publish operation.</returns>
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, PublishBranchForceOptions forceOptions, Guid userKey)
#pragma warning disable CS0618 // Type or member is obsolete
        => PublishBranchAsync(key, cultures, forceOptions.HasFlag(PublishBranchForceOptions.PublishUnpublished), userKey);
#pragma warning restore CS0618 // Type or member is obsolete

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
