using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberPresentationFactory : IMemberPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IMemberService _memberService;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    public MemberPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IMemberService memberService,
        ITwoFactorLoginService twoFactorLoginService)
    {
        _umbracoMapper = umbracoMapper;
        _memberService = memberService;
        _twoFactorLoginService = twoFactorLoginService;
    }

    public async Task<MemberResponseModel> CreateResponseModelAsync(IMember member)
    {
        MemberResponseModel responseModel = _umbracoMapper.Map<MemberResponseModel>(member)!;

        responseModel.IsTwoFactorEnabled = await _twoFactorLoginService.IsTwoFactorEnabledAsync(member.Key);
        responseModel.Groups = _memberService.GetAllRoles(member.Username);

        return responseModel;
    }

    public async Task<IEnumerable<MemberResponseModel>> CreateMultipleAsync(IEnumerable<IMember> members)
    {
        var memberResponseModels = new List<MemberResponseModel>();
        foreach (IMember member in members)
        {
            memberResponseModels.Add(await CreateResponseModelAsync(member));
        }

        return memberResponseModels;
    }

    public MemberItemResponseModel CreateItemResponseModel(IMemberEntitySlim entity)
    {
        var responseModel = new MemberItemResponseModel
        {
            Id = entity.Key,
        };

        responseModel.MemberType = _umbracoMapper.Map<MemberTypeReferenceResponseModel>(entity)!;

        responseModel.Variants = CreateVariantsItemResponseModels(entity);

        return responseModel;
    }

    public IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IMemberEntitySlim entity)
        => new[]
        {
            new VariantItemResponseModel
            {
                Name = entity.Name ?? string.Empty,
                Culture = null
            }
        };

    public MemberTypeReferenceResponseModel CreateMemberTypeReferenceResponseModel(IMemberEntitySlim entity)
        => _umbracoMapper.Map<MemberTypeReferenceResponseModel>(entity)!;
}
