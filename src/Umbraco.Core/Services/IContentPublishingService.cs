using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentPublishingService
{

    Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id,  Guid userKey, string culture = "*");

    Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id,  Guid userKey, string[] cultures);
}
