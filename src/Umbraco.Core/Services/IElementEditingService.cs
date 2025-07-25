using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

// TODO ELEMENTS: fully define this interface
public interface IElementEditingService
{
    Task<IElement?> GetAsync(Guid key);

    Task<Attempt<ElementCreateResult, ContentEditingOperationStatus>> CreateAsync(ElementCreateModel createModel, Guid userKey);

    Task<Attempt<ElementUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ElementUpdateModel updateModel, Guid userKey);

    Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);
}
