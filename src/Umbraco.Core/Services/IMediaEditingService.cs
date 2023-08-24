using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IMediaEditingService
{
    Task<IMedia?> GetAsync(Guid key);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, Guid userKey);

    Task<Attempt<IMedia, ContentEditingOperationStatus>> UpdateAsync(IMedia media, MediaUpdateModel updateModel, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey);
}
