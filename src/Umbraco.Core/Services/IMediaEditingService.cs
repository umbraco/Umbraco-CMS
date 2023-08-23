using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IMediaEditingService
{
    Task<IMedia?> GetAsync(Guid id);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, Guid userKey);

    Task<Attempt<IMedia, ContentEditingOperationStatus>> UpdateAsync(IMedia content, MediaUpdateModel updateModel, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid id, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid id, Guid? parentId, Guid userKey);

    Task<ContentEditingOperationStatus> SortAsync(Guid? parentId, IEnumerable<SortingModel> sortingModels, Guid userKey);
}
