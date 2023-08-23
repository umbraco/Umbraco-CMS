using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentEditingService
{
    Task<IContent?> GetAsync(Guid key);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, Guid userKey);

    Task<Attempt<IContent, ContentEditingOperationStatus>> UpdateAsync(IContent content, ContentUpdateModel updateModel, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey);

    Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey);
}
