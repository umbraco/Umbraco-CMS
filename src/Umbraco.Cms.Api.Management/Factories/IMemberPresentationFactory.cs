using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMemberPresentationFactory
{
    Task<MemberResponseModel> CreateResponseModelAsync(IMember member);

    Task<IEnumerable<MemberResponseModel>> CreateMultipleAsync(IEnumerable<IMember> members);

    MemberItemResponseModel CreateItemResponseModel(IMemberEntitySlim entity);

    IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IMemberEntitySlim entity);

    MemberTypeReferenceResponseModel CreateMemberTypeReferenceResponseModel(IMemberEntitySlim entity);
}
