using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IMemberEditingService
{
    Task<IMember?> GetAsync(Guid key);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(MemberCreateModel createModel);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MemberUpdateModel updateModel);

    Task<Attempt<MemberCreateResult, MemberEditingStatus>> CreateAsync(MemberCreateModel createModel, IUser user);

    Task<Attempt<MemberUpdateResult, MemberEditingStatus>> UpdateAsync(Guid key, MemberUpdateModel updateModel, IUser user);

    Task<Attempt<IMember?, MemberEditingStatus>> DeleteAsync(Guid key, Guid userKey);
}
