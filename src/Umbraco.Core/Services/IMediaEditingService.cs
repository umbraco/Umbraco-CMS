using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IMediaEditingService
{
    Task<IMedia?> GetAsync(Guid key);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(MediaCreateModel createModel);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MediaUpdateModel updateModel);

    Task<Attempt<MediaCreateResult, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, Guid userKey);

    Task<Attempt<MediaUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, MediaUpdateModel updateModel, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey);
    Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey);
}
