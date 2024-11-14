using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;

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
    Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, bool force, Guid userKey);

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
    async Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        Guid key,
        ICollection<CulturePublishScheduleModel> culturesToPublishOrSchedule,
        Guid userKey)
    {
        // todo remove default implementation when superseded method is removed in v17+
        var culturesToPublishImmediately =
            culturesToPublishOrSchedule.Where(culture => culture.Schedule is null).Select(c => c.Culture ?? Constants.System.InvariantCulture).ToHashSet();

        ContentScheduleCollection schedules = StaticServiceProvider.Instance.GetRequiredService<IContentService>().GetContentScheduleByContentId(key);

        foreach (CulturePublishScheduleModel cultureToSchedule in culturesToPublishOrSchedule.Where(c => c.Schedule is not null))
        {
            var culture = cultureToSchedule.Culture ?? Constants.System.InvariantCulture;

            if (cultureToSchedule.Schedule!.PublishDate is null)
            {
                schedules.RemoveIfExists(culture, ContentScheduleAction.Release);
            }
            else
            {
                schedules.AddOrUpdate(culture, cultureToSchedule.Schedule!.PublishDate.Value.UtcDateTime,ContentScheduleAction.Release);
            }

            if (cultureToSchedule.Schedule!.UnpublishDate is null)
            {
                schedules.RemoveIfExists(culture, ContentScheduleAction.Expire);
            }
            else
            {
                schedules.AddOrUpdate(culture, cultureToSchedule.Schedule!.UnpublishDate.Value.UtcDateTime, ContentScheduleAction.Expire);
            }
        }

        var cultureAndSchedule = new CultureAndScheduleModel
        {
            CulturesToPublishImmediately = culturesToPublishImmediately,
            Schedules = schedules,
        };

#pragma warning disable CS0618 // Type or member is obsolete
        return await PublishAsync(key, cultureAndSchedule, userKey);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
