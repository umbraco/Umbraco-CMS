﻿using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberPresentationFactory : IMemberPresentationFactory
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
    {
        _umbracoMapper = umbracoMapper;
        _memberService = memberService;
        _memberTypeService = memberTypeService;
        _twoFactorLoginService = twoFactorLoginService;
    }

    public async Task<MemberResponseModel> CreateResponseModelAsync(IMember member, IUser currentUser)
    {
        MemberResponseModel responseModel = _umbracoMapper.Map<MemberResponseModel>(member)!;

        responseModel.IsTwoFactorEnabled = await _twoFactorLoginService.IsTwoFactorEnabledAsync(member.Key);
        responseModel.Groups = _memberService.GetAllRoles(member.Username);

        return currentUser.HasAccessToSensitiveData()
            ? responseModel
            : await RemoveSensitiveDataAsync(member, responseModel);
    }

    public async Task<IEnumerable<MemberResponseModel>> CreateMultipleAsync(IEnumerable<IMember> members, IUser currentUser)
    {
        var memberResponseModels = new List<MemberResponseModel>();
        foreach (IMember member in members)
        {
            memberResponseModels.Add(await CreateResponseModelAsync(member, currentUser));
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

    private async Task<MemberResponseModel> RemoveSensitiveDataAsync(IMember member, MemberResponseModel responseModel)
    {
        // these properties are considered sensitive; some of them are not nullable, so for
        // those we can't do much more than force revert them to their default values.
        responseModel.IsApproved = false;
        responseModel.IsLockedOut = false;
        responseModel.IsTwoFactorEnabled = false;
        responseModel.FailedPasswordAttempts = 0;
        responseModel.LastLoginDate = null;
        responseModel.LastLockoutDate = null;
        responseModel.LastPasswordChangeDate = null;

        IMemberType memberType = await _memberTypeService.GetAsync(member.ContentType.Key)
                                 ?? throw new InvalidOperationException($"The member type {member.ContentType.Alias} could not be found");

        var sensitivePropertyAliases = memberType.GetSensitivePropertyTypeAliases().ToArray();

        // remove all properties whose property types are flagged as sensitive
        responseModel.Values = responseModel.Values
            .Where(valueModel => sensitivePropertyAliases.InvariantContains(valueModel.Alias) is false)
            .ToArray();

        return responseModel;
    }
}
