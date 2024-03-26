using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentBlueprintEditingService
{
    Task<IContent?> GetAsync(Guid key);

    Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAsync(ContentBlueprintCreateModel createModel, Guid userKey);

    Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateFromContentAsync(Guid contentKey, string name, Guid userKey);

    Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentBlueprintUpdateModel updateModel, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);
}
