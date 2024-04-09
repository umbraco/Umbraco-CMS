using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public interface IMemberTypeEditingService
{
    Task<Attempt<IMemberType?, ContentTypeOperationStatus>> CreateAsync(MemberTypeCreateModel model, Guid userKey);

    Task<Attempt<IMemberType?, ContentTypeOperationStatus>> UpdateAsync(IMemberType memberType, MemberTypeUpdateModel model, Guid userKey);

    Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases);
}
