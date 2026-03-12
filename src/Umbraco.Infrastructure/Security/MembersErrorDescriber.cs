using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Provides descriptive error messages for operations related to Umbraco members.
/// </summary>
public class MembersErrorDescriber : UmbracoErrorDescriberBase
{
    private readonly ILocalizedTextService _textService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Security.MembersErrorDescriber"/> class with the specified localized text service.
    /// </summary>
    /// <param name="textService">An instance of <see cref="ILocalizedTextService"/> used to provide localized error messages.</param>
    public MembersErrorDescriber(ILocalizedTextService textService)
        : base(textService) => _textService = textService;

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that a role name already exists.
    /// </summary>
    /// <param name="role">The role name that is already in use.</param>
    /// <returns>An <see cref="IdentityError"/> describing the error for a duplicate role name.</returns>
    public override IdentityError DuplicateRoleName(string role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = _textService.Localize("validation", "duplicateMemberGroupName", new[] { role }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the specified role name is invalid.
    /// </summary>
    /// <param name="role">The invalid role name.</param>
    /// <returns>An <see cref="IdentityError"/> describing the invalid role name error.</returns>
    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = _textService.Localize("validation", "invalidMemberGroupName"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the login is already associated with another member.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the duplicate login association error.</returns>
    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = _textService.Localize("member", "duplicateMemberLogin"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the user already has a password.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the error.</returns>
    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = _textService.Localize("member", "memberHasPassword"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the user is already a member of the specified role.
    /// </summary>
    /// <param name="role">The name of the role the user is already in.</param>
    /// <returns>An <see cref="IdentityError"/> that describes the error.</returns>
    public override IdentityError UserAlreadyInRole(string role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = _textService.Localize("member", "memberHasGroup", new[] { role }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that user lockout is not enabled.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the user lockout not enabled error.</returns>
    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = _textService.Localize("member", "memberLockoutNotEnabled"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating the user is not in the specified role.
    /// </summary>
    /// <param name="role">The role to check membership for.</param>
    /// <returns>An <see cref="IdentityError"/> describing the error.</returns>
    public override IdentityError UserNotInRole(string role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = _textService.Localize("member", "memberNotInGroup", new[] { role }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the provided password does not match.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the password mismatch error.</returns>
    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = _textService.Localize("member", "incorrectPassword"),
    };
}
