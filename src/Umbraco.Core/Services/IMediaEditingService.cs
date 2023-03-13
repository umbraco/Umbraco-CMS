using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IMediaEditingService
{
    Task<IMedia?> GetAsync(Guid id);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, int userId = Constants.Security.SuperUserId);

    Task<Attempt<IMedia, ContentEditingOperationStatus>> UpdateAsync(IMedia content, MediaUpdateModel updateModel, int userId = Constants.Security.SuperUserId);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, int userId = Constants.Security.SuperUserId);

    Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId);
}
