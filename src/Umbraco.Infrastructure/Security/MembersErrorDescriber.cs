using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

public class MembersErrorDescriber : UmbracoErrorDescriberBase
{
    private readonly ILocalizedTextService _textService;

    public MembersErrorDescriber(ILocalizedTextService textService)
        : base(textService) => _textService = textService;

    public override IdentityError DuplicateRoleName(string role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = _textService.Localize("validation", "duplicateMemberGroupName", new[] { role }),
    };

    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = _textService.Localize("validation", "invalidMemberGroupName"),
    };

    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = _textService.Localize("member", "duplicateMemberLogin"),
    };

    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = _textService.Localize("member", "memberHasPassword"),
    };

    public override IdentityError UserAlreadyInRole(string role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = _textService.Localize("member", "memberHasGroup", new[] { role }),
    };

    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = _textService.Localize("member", "memberLockoutNotEnabled"),
    };

    public override IdentityError UserNotInRole(string role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = _textService.Localize("member", "memberNotInGroup", new[] { role }),
    };

    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = _textService.Localize("member", "incorrectPassword"),
    };
}
