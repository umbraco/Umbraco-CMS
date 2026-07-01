using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Serves as the base class for creating custom error describers related to Umbraco security operations.
/// Inherit from this class to provide localized or customized error messages for security-related functionality.
/// </summary>
public abstract class UmbracoErrorDescriberBase : IdentityErrorDescriber
{
    private readonly ILocalizedTextService _textService;

    protected UmbracoErrorDescriberBase(ILocalizedTextService textService) => _textService = textService;

    /// <summary>
    /// Returns an <see cref="IdentityError"/> that indicates a concurrency failure occurred, such as when a data update conflict is detected.
    /// </summary>
    /// <returns>
    /// An <see cref="IdentityError"/> describing the concurrency failure error.
    /// </returns>
    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = _textService.Localize("errors", "concurrencyError"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> that represents a general or unspecified error.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> with a default error code and a localized description.</returns>
    public override IdentityError DefaultError() => new()
    {
        Code = nameof(DefaultError),
        Description = _textService.Localize("errors", "defaultError"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the specified email address is already in use.
    /// </summary>
    /// <param name="email">The email address that is duplicated.</param>
    /// <returns>An <see cref="IdentityError"/> describing the duplicate email error.</returns>
    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = _textService.Localize("validation", "duplicateEmail", new[] { email }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the specified user name already exists.
    /// </summary>
    /// <param name="userName">The user name that is duplicated.</param>
    /// <returns>An <see cref="IdentityError"/> describing the duplicate user name error.</returns>
    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = _textService.Localize("validation", "duplicateUsername", new[] { userName }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating the specified email is invalid.
    /// </summary>
    /// <param name="email">The email address that is invalid.</param>
    /// <returns>An <see cref="IdentityError"/> describing the invalid email error.</returns>
    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = _textService.Localize("validation", "invalidEmail"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the token is invalid.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the invalid token error.</returns>
    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = _textService.Localize("validation", "invalidToken"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the specified user name is invalid.
    /// </summary>
    /// <param name="userName">The user name that is invalid.</param>
    /// <returns>An <see cref="IdentityError"/> describing the invalid user name error.</returns>
    public override IdentityError InvalidUserName(string? userName) => new()
    {
        Code = nameof(InvalidUserName),
        Description = _textService.Localize("validation", "invalidUsername"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the provided password is incorrect.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the password mismatch.</returns>
    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = _textService.Localize("user", "passwordMismatch"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the password requires at least one digit.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the digit requirement for the password.</returns>
    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = _textService.Localize("user", "passwordRequiresDigit"),
    };

    /// <summary>Returns an <see cref="IdentityError"/> indicating that the password requires a lowercase letter.</summary>
    /// <returns>An <see cref="IdentityError"/> describing the lowercase letter requirement.</returns>
    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = _textService.Localize("user", "passwordRequiresLower"),
    };

    /// <summary>Returns an <see cref="IdentityError"/> indicating that the password requires a non-alphanumeric character.</summary>
    /// <returns>An <see cref="IdentityError"/> describing the non-alphanumeric password requirement.</returns>
    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = _textService.Localize("user", "passwordRequiresNonAlphanumeric"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the password must contain at least a specified number of unique characters.
    /// </summary>
    /// <param name="uniqueChars">The minimum number of unique characters required in the password.</param>
    /// <returns>An <see cref="IdentityError"/> describing the unique character requirement for the password.</returns>
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = _textService.Localize("user", "passwordRequiresUniqueChars", new[] { uniqueChars.ToString() }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the password requires at least one uppercase letter.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the uppercase letter requirement.</returns>
    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = _textService.Localize("user", "passwordRequiresUpper"),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that the provided password is too short.
    /// </summary>
    /// <param name="length">The minimum required length for the password.</param>
    /// <returns>An <see cref="IdentityError"/> describing the password length requirement.</returns>
    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = _textService.Localize("user", "passwordTooShort", new[] { length.ToString() }),
    };

    /// <summary>
    /// Returns an <see cref="IdentityError"/> indicating that redemption of a recovery code has failed, typically due to expiration or invalidity.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> describing the failure to redeem a recovery code.</returns>
    public override IdentityError RecoveryCodeRedemptionFailed() => new()
    {
        Code = nameof(RecoveryCodeRedemptionFailed),
        Description = _textService.Localize("login", "resetCodeExpired"),
    };
}
