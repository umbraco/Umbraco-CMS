using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Umbraco back office specific <see cref="IdentityErrorDescriber" />
/// </summary>
public class BackOfficeErrorDescriber : UmbracoErrorDescriberBase
{
    private readonly ILocalizedTextService _textService;

    public BackOfficeErrorDescriber(ILocalizedTextService textService)
        : base(textService) => _textService = textService;

    public override IdentityError DuplicateRoleName(string role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = _textService.Localize("validation", "duplicateUserGroupName", new[] { role }),
    };

    public override IdentityError InvalidRoleName(string role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = _textService.Localize("validation", "invalidUserGroupName"),
    };

    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = _textService.Localize("user", "duplicateLogin"),
    };

    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = _textService.Localize("user", "userHasPassword"),
    };

    public override IdentityError UserAlreadyInRole(string role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = _textService.Localize("user", "userHasGroup", new[] { role }),
    };

    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = _textService.Localize("user", "userLockoutNotEnabled"),
    };

    public override IdentityError UserNotInRole(string role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = _textService.Localize("user", "userNotInGroup", new[] { role }),
    };
}

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

    public override IdentityError InvalidRoleName(string role) => new()
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
}
