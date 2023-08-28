using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentPublishingService
{

    Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id,  Guid userKey, string culture = "*");

    Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id,  Guid userKey, string[] cultures);

    Task<Attempt<ContentPublishingOperationStatus>> PublishBranch(Guid id, bool force, Guid userKey, string culture = "*");
    Task<Attempt<ContentPublishingOperationStatus>> PublishBranch(Guid id, bool force, Guid userKey, string[] cultures);
}
