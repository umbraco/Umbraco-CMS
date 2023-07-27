using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentEditingService
{
    Task<IContent?> GetAsync(Guid id);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, Guid userKey);

    Task<Attempt<IContent, ContentEditingOperationStatus>> UpdateAsync(IContent content, ContentUpdateModel updateModel, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid id, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid id, Guid? parentId, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid id, Guid? parentId, bool relateToOriginal, bool includeDescendants, Guid userKey);

    Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id,  Guid userKey, string culture = "*");

    Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id,  Guid userKey, string[] cultures);
}
