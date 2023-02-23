using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentEditingService
{
    Task<IContent?> GetAsync(Guid id);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, int userId = Constants.Security.SuperUserId);

    Task<Attempt<IContent, ContentEditingOperationStatus>> UpdateAsync(IContent content, ContentUpdateModel updateModel, int userId = Constants.Security.SuperUserId);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, int userId = Constants.Security.SuperUserId);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId);
}
