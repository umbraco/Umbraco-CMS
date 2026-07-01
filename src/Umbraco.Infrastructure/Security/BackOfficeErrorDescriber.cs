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

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeErrorDescriber"/> class with the specified localized text service.
    /// </summary>
    /// <param name="textService">An instance of <see cref="ILocalizedTextService"/> used for localization.</param>
    public BackOfficeErrorDescriber(ILocalizedTextService textService)
        : base(textService) => _textService = textService;

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that a role name is already in use.
    /// </summary>
    /// <param name="role">The name of the role that is duplicated.</param>
    /// <returns>An <see cref="IdentityError"/> describing the duplicate role name error.</returns>
    public override IdentityError DuplicateRoleName(string role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = _textService.Localize("validation", "duplicateUserGroupName", new[] { role }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the specified role name is invalid.
    /// This error typically occurs when the provided role name does not meet the required validation rules.
    /// </summary>
    /// <param name="role">The role name that failed validation.</param>
    /// <returns>An <see cref="IdentityError"/> describing the invalid role name error.</returns>
    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = _textService.Localize("validation", "invalidUserGroupName"),
    };

    /// <summary>Returns an <see cref="IdentityError"/> indicating that the login is already associated with another user.</summary>
    /// <returns>An <see cref="IdentityError"/> describing the duplicate login error.</returns>
    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = _textService.Localize("user", "duplicateLogin"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the user already has a password set.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the error that the user already has a password.</returns>
    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = _textService.Localize("user", "userHasPassword"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating the user is already in the specified role.
    /// </summary>
    /// <param name="role">The name of the role to check.</param>
    /// <returns>An <see cref="IdentityError"/> describing the error.</returns>
    public override IdentityError UserAlreadyInRole(string role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = _textService.Localize("user", "userHasGroup", new[] { role }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that lockout is not enabled for the user.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the error that user lockout is not enabled.</returns>
    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = _textService.Localize("user", "userLockoutNotEnabled"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the user is not in the specified role.
    /// </summary>
    /// <param name="role">The role that the user is not a member of.</param>
    /// <returns>An <see cref="IdentityError"/> describing the error.</returns>
    public override IdentityError UserNotInRole(string role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = _textService.Localize("user", "userNotInGroup", new[] { role }),
    };
}
