using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMemberEditingPresentationFactory
{
    MemberCreateModel MapCreateModel(CreateMemberRequestModel createRequestModel);

    MemberUpdateModel MapUpdateModel(UpdateMemberRequestModel updateRequestModel);
}
