using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMemberPresentationFactory
{
    Task<MemberResponseModel> CreateResponseModelAsync(IMember member, IUser currentUser);

    Task<IEnumerable<MemberResponseModel>> CreateMultipleAsync(IEnumerable<IMember> members, IUser currentUser);

    MemberItemResponseModel CreateItemResponseModel(IMemberEntitySlim entity);

    MemberItemResponseModel CreateItemResponseModel(IMember entity);
}
