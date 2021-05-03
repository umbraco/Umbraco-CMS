using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// Umbraco back office specific <see cref="IdentityErrorDescriber"/>
    /// </summary>
    public class BackOfficeErrorDescriber : IdentityErrorDescriber
    {
        private readonly ILocalizedTextService _textService;

        public BackOfficeErrorDescriber(ILocalizedTextService textService) => _textService = textService;

        // Overriding all methods in order to provide our own translated error messages

        public override IdentityError ConcurrencyFailure() => new IdentityError
        {
            Code = nameof(ConcurrencyFailure),
            Description = _textService.Localize("errors/concurrencyError")
        };

        public override IdentityError DefaultError() => new IdentityError
        {
            Code = nameof(DefaultError),
            Description = _textService.Localize("errors/defaultError")
        };

        public override IdentityError DuplicateEmail(string email) => new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = _textService.Localize("validation/duplicateEmail", new[] { email })
        };

        public override IdentityError DuplicateRoleName(string role) => new IdentityError
        {
            Code = nameof(DuplicateRoleName),
            Description = _textService.Localize("validation/duplicateUserGroupName", new[] { role })
        };

        public override IdentityError DuplicateUserName(string userName) => new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = _textService.Localize("validation/duplicateUsername", new[] { userName })
        };

        public override IdentityError InvalidEmail(string email) => new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = _textService.Localize("validation/invalidEmail")
        };

        public override IdentityError InvalidRoleName(string role) => new IdentityError
        {
            Code = nameof(InvalidRoleName),
            Description = _textService.Localize("validation/invalidUserGroupName")
        };

        public override IdentityError InvalidToken() => new IdentityError
        {
            Code = nameof(InvalidToken),
            Description = _textService.Localize("validation/invalidToken")
        };

        public override IdentityError InvalidUserName(string userName) => new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = _textService.Localize("validation/invalidUsername")
        };

        public override IdentityError LoginAlreadyAssociated() => new IdentityError
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = _textService.Localize("user/duplicateLogin")
        };

        public override IdentityError PasswordMismatch() => new IdentityError
        {
            Code = nameof(PasswordMismatch),
            Description = _textService.Localize("user/passwordMismatch")
        };

        public override IdentityError PasswordRequiresDigit() => new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = _textService.Localize("user/passwordRequiresDigit")
        };

        public override IdentityError PasswordRequiresLower() => new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = _textService.Localize("user/passwordRequiresLower")
        };

        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = _textService.Localize("user/passwordRequiresNonAlphanumeric")
        };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new IdentityError
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = _textService.Localize("user/passwordRequiresUniqueChars", new[] { uniqueChars.ToString() })
        };

        public override IdentityError PasswordRequiresUpper() => new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = _textService.Localize("user/passwordRequiresUpper")
        };

        public override IdentityError PasswordTooShort(int length) => new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = _textService.Localize("user/passwordTooShort", new[] { length.ToString() })
        };

        public override IdentityError RecoveryCodeRedemptionFailed() => new IdentityError
        {
            Code = nameof(RecoveryCodeRedemptionFailed),
            Description = _textService.Localize("login/resetCodeExpired")
        };

        public override IdentityError UserAlreadyHasPassword() => new IdentityError
        {
            Code = nameof(UserAlreadyHasPassword),
            Description = _textService.Localize("user/userHasPassword")
        };

        public override IdentityError UserAlreadyInRole(string role) => new IdentityError
        {
            Description = _textService.Localize("user/userHasGroup", new[] { role })
        };

        public override IdentityError UserLockoutNotEnabled() => new IdentityError
        {
            Code = nameof(UserLockoutNotEnabled),
            Description = _textService.Localize("user/userLockoutNotEnabled")
        };

        public override IdentityError UserNotInRole(string role) => new IdentityError
        {
            Code = nameof(UserNotInRole),
            Description = _textService.Localize("user/userNotInGroup", new[] { role })
        };
    }

    public class MembersErrorDescriber : IdentityErrorDescriber
    {
        private readonly ILocalizedTextService _textService;

        public MembersErrorDescriber(ILocalizedTextService textService) => _textService = textService;

        // Overriding all methods in order to provide our own translated error messages

        public override IdentityError ConcurrencyFailure() => new IdentityError
        {
            Code = nameof(ConcurrencyFailure),
            Description = _textService.Localize("errors/concurrencyError")
        };

        public override IdentityError DefaultError() => new IdentityError
        {
            Code = nameof(DefaultError),
            Description = _textService.Localize("errors/defaultError")
        };

        public override IdentityError DuplicateEmail(string email) => new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = _textService.Localize("validation/duplicateEmail", new[] { email })
        };

        public override IdentityError DuplicateRoleName(string role) => new IdentityError
        {
            Code = nameof(DuplicateRoleName),
            Description = _textService.Localize("validation/duplicateMemberGroupName", new[] { role })
        };

        public override IdentityError DuplicateUserName(string userName) => new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = _textService.Localize("validation/duplicateUsername", new[] { userName })
        };

        public override IdentityError InvalidEmail(string email) => new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = _textService.Localize("validation/invalidEmail")
        };

        public override IdentityError InvalidRoleName(string role) => new IdentityError
        {
            Code = nameof(InvalidRoleName),
            Description = _textService.Localize("validation/invalidMemberGroupName")
        };

        public override IdentityError InvalidToken() => new IdentityError
        {
            Code = nameof(InvalidToken),
            Description = _textService.Localize("validation/invalidToken")
        };

        public override IdentityError InvalidUserName(string userName) => new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = _textService.Localize("validation/invalidUsername")
        };

        public override IdentityError LoginAlreadyAssociated() => new IdentityError
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = _textService.Localize("member/duplicateMemberLogin")
        };

        public override IdentityError PasswordMismatch() => new IdentityError
        {
            Code = nameof(PasswordMismatch),
            Description = _textService.Localize("user/passwordMismatch")
        };

        public override IdentityError PasswordRequiresDigit() => new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = _textService.Localize("user/passwordRequiresDigit")
        };

        public override IdentityError PasswordRequiresLower() => new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = _textService.Localize("user/passwordRequiresLower")
        };

        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = _textService.Localize("user/passwordRequiresNonAlphanumeric")
        };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new IdentityError
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = _textService.Localize("user/passwordRequiresUniqueChars", new[] { uniqueChars.ToString() })
        };

        public override IdentityError PasswordRequiresUpper() => new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = _textService.Localize("user/passwordRequiresUpper")
        };

        public override IdentityError PasswordTooShort(int length) => new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = _textService.Localize("user/passwordTooShort", new[] { length.ToString() })
        };

        public override IdentityError RecoveryCodeRedemptionFailed() => new IdentityError
        {
            Code = nameof(RecoveryCodeRedemptionFailed),
            Description = _textService.Localize("login/resetCodeExpired")
        };

        public override IdentityError UserAlreadyHasPassword() => new IdentityError
        {
            Code = nameof(UserAlreadyHasPassword),
            Description = _textService.Localize("member/memberHasPassword")
        };

        public override IdentityError UserAlreadyInRole(string role) => new IdentityError
        {
            Code = nameof(UserAlreadyInRole),
            Description = _textService.Localize("member/memberHasGroup", new[] { role })
        };

        public override IdentityError UserLockoutNotEnabled() => new IdentityError
        {
            Code = nameof(UserLockoutNotEnabled),
            Description = _textService.Localize("member/memberLockoutNotEnabled")
        };

        public override IdentityError UserNotInRole(string role) => new IdentityError
        {
            Code = nameof(UserNotInRole),
            Description = _textService.Localize("member/memberNotInGroup", new[] { role })
        };
    }
}
