using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

// TODO ELEMENTS: fully define this interface
public interface IElementEditingService
{
    Task<IElement?> GetAsync(Guid key);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ElementCreateModel createModel, Guid userKey);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateElementUpdateModel updateModel, Guid userKey);

    Task<Attempt<ElementCreateResult, ContentEditingOperationStatus>> CreateAsync(ElementCreateModel createModel, Guid userKey);

    Task<Attempt<ElementUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ElementUpdateModel updateModel, Guid userKey);

    Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    Task<Attempt<IElement?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? containerKey, Guid userKey);

    Task<Attempt<IElement?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, Guid userKey);

    Task<Attempt<ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<IElement?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? containerKey, Guid userKey);
}
