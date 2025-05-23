using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IMemberContentEditingService
{
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateAsync(MemberEditingModelBase editingModel, Guid memberTypeKey);

    Task<Attempt<MemberUpdateResult, ContentEditingOperationStatus>> UpdateAsync(IMember member, MemberEditingModelBase updateModel, Guid userKey);

    Task<Attempt<IMember?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);
}
