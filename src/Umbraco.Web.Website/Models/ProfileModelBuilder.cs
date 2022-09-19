using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Web.Website.Models;

public class ProfileModelBuilder : MemberModelBuilderBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemberService _memberService;
    private bool _lookupProperties;
    private string? _redirectUrl;

    public ProfileModelBuilder(
        IMemberTypeService memberTypeService,
        IMemberService memberService,
        IShortStringHelper shortStringHelper,
        IHttpContextAccessor httpContextAccessor)
        : base(memberTypeService, shortStringHelper)
    {
        _memberService = memberService;
        _httpContextAccessor = httpContextAccessor;
    }

    public ProfileModelBuilder WithRedirectUrl(string redirectUrl)
    {
        _redirectUrl = redirectUrl;
        return this;
    }

    public ProfileModelBuilder WithCustomProperties(bool lookupProperties)
    {
        _lookupProperties = lookupProperties;
        return this;
    }

    public async Task<ProfileModel?> BuildForCurrentMemberAsync()
    {
        IMemberManager? memberManager =
            _httpContextAccessor.HttpContext?.RequestServices.GetRequiredService<IMemberManager>();

        if (memberManager == null)
        {
            return null;
        }

        MemberIdentityUser? member = _httpContextAccessor.HttpContext is null
            ? null
            : await memberManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

        if (member == null)
        {
            return null;
        }

        var model = new ProfileModel
        {
            Name = member.Name,
            Email = member.Email,
            UserName = member.UserName,
            Comments = member.Comments,
            IsApproved = member.IsApproved,
            IsLockedOut = member.IsLockedOut,
            LastLockoutDate = member.LastLockoutDateUtc?.ToLocalTime(),
            CreatedDate = member.CreatedDateUtc.ToLocalTime(),
            LastLoginDate = member.LastLoginDateUtc?.ToLocalTime(),
            LastPasswordChangedDate = member.LastPasswordChangeDateUtc?.ToLocalTime(),
            RedirectUrl = _redirectUrl,
            Key = member.Key,
        };

        IMemberType? memberType = member.MemberTypeAlias is null ? null : MemberTypeService.Get(member.MemberTypeAlias);
        if (memberType == null)
        {
            throw new InvalidOperationException($"Could not find a member type with alias: {member.MemberTypeAlias}.");
        }

        // TODO: This wouldn't be required if we support exposing custom member properties on the MemberIdentityUser at the ASP.NET Identity level.
        IMember? persistedMember = _memberService.GetByKey(member.Key);
        if (persistedMember == null)
        {
            // should never happen
            throw new InvalidOperationException($"Could not find a member with key: {member.Key}.");
        }

        if (_lookupProperties)
        {
            model.MemberProperties = GetMemberPropertiesViewModel(memberType, persistedMember);
        }

        return model;
    }
}
