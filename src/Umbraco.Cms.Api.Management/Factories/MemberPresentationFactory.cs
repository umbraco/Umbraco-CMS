using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberPresentationFactory
    : ContentPresentationFactoryBase<IMemberType, IMemberTypeService>, IMemberPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IMemberService _memberService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    public MemberPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        ITwoFactorLoginService twoFactorLoginService)
        : base(memberTypeService, umbracoMapper)
    {
        _umbracoMapper = umbracoMapper;
        _memberService = memberService;
        _memberTypeService = memberTypeService;
        _twoFactorLoginService = twoFactorLoginService;
    }

    public async Task<MemberResponseModel> CreateResponseModelAsync(IMember member)
    {
        MemberResponseModel responseModel = _umbracoMapper.Map<MemberResponseModel>(member)!;

        responseModel.IsTwoFactorEnabled = await _twoFactorLoginService.IsTwoFactorEnabledAsync(member.Key);
        responseModel.Groups = _memberService.GetAllRoles(member.Username);

        return responseModel;
    }

    public MemberItemResponseModel CreateItemResponseModel(IMemberEntitySlim entity)
    {
        var responseModel = new MemberItemResponseModel
        {
            Id = entity.Key,
        };

        IMemberType? memberType = _memberTypeService.Get(entity.ContentTypeAlias);
        if (memberType is not null)
        {
            responseModel.MemberType = _umbracoMapper.Map<MemberTypeReferenceResponseModel>(memberType)!;
        }

        // TODO: does this make sense?
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
        => CreateContentTypeReferenceResponseModel<MemberTypeReferenceResponseModel>(entity);
}
