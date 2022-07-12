using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

public abstract class UmbracoErrorDescriberBase : IdentityErrorDescriber
{
    private readonly ILocalizedTextService _textService;

    protected UmbracoErrorDescriberBase(ILocalizedTextService textService) => _textService = textService;

    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = _textService.Localize("errors", "concurrencyError"),
    };

    public override IdentityError DefaultError() => new()
    {
        Code = nameof(DefaultError),
        Description = _textService.Localize("errors", "defaultError"),
    };

    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = _textService.Localize("validation", "duplicateEmail", new[] { email }),
    };

    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = _textService.Localize("validation", "duplicateUsername", new[] { userName }),
    };

    public override IdentityError InvalidEmail(string email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = _textService.Localize("validation", "invalidEmail"),
    };

    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = _textService.Localize("validation", "invalidToken"),
    };

    public override IdentityError InvalidUserName(string userName) => new()
    {
        Code = nameof(InvalidUserName),
        Description = _textService.Localize("validation", "invalidUsername"),
    };

    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = _textService.Localize("user", "passwordMismatch"),
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = _textService.Localize("user", "passwordRequiresDigit"),
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = _textService.Localize("user", "passwordRequiresLower"),
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = _textService.Localize("user", "passwordRequiresNonAlphanumeric"),
    };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = _textService.Localize("user", "passwordRequiresUniqueChars", new[] { uniqueChars.ToString() }),
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = _textService.Localize("user", "passwordRequiresUpper"),
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = _textService.Localize("user", "passwordTooShort", new[] { length.ToString() }),
    };

    public override IdentityError RecoveryCodeRedemptionFailed() => new()
    {
        Code = nameof(RecoveryCodeRedemptionFailed),
        Description = _textService.Localize("login", "resetCodeExpired"),
    };
}
