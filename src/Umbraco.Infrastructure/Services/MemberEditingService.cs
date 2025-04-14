using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal sealed class MemberEditingService : IMemberEditingService
{
    private readonly IMemberService _memberService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberContentEditingService _memberContentEditingService;
    private readonly IMemberManager _memberManager;
    private readonly ITwoFactorLoginService _twoFactorLoginService;
    private readonly IPasswordChanger<MemberIdentityUser> _passwordChanger;
    private readonly ILogger<MemberEditingService> _logger;
    private readonly IMemberGroupService _memberGroupService;
    private readonly SecuritySettings _securitySettings;

    public MemberEditingService(
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberContentEditingService memberContentEditingService,
        IMemberManager memberManager,
        ITwoFactorLoginService twoFactorLoginService,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        ILogger<MemberEditingService> logger,
        IMemberGroupService memberGroupService,
        IOptions<SecuritySettings> securitySettings)
    {
        _memberService = memberService;
        _memberTypeService = memberTypeService;
        _memberContentEditingService = memberContentEditingService;
        _memberManager = memberManager;
        _twoFactorLoginService = twoFactorLoginService;
        _passwordChanger = passwordChanger;
        _logger = logger;
        _memberGroupService = memberGroupService;
        _securitySettings = securitySettings.Value;
    }

    public Task<IMember?> GetAsync(Guid key)
        => Task.FromResult(_memberService.GetById(key));

    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(MemberCreateModel createModel)
        => await _memberContentEditingService.ValidateAsync(createModel, createModel.ContentTypeKey);

    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MemberUpdateModel updateModel)
    {
        IMember? member = _memberService.GetById(key);
        return member is not null
            ? await _memberContentEditingService.ValidateAsync(updateModel, member.ContentType.Key)
            : Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentValidationResult());
    }

    public async Task<Attempt<MemberCreateResult, MemberEditingStatus>> CreateAsync(MemberCreateModel createModel, IUser user)
    {
        var status = new MemberEditingStatus();

        MemberEditingOperationStatus validationStatus = await ValidateMemberDataAsync(createModel, null, createModel.Password);
        if (validationStatus is not MemberEditingOperationStatus.Success)
        {
            status.MemberEditingOperationStatus = validationStatus;
            return Attempt.FailWithStatus(status, new MemberCreateResult());
        }

        IMemberType? memberType = await _memberTypeService.GetAsync(createModel.ContentTypeKey);
        if (memberType is null)
        {
            status.MemberEditingOperationStatus = MemberEditingOperationStatus.MemberTypeNotFound;
            return Attempt.FailWithStatus(status, new MemberCreateResult());
        }

        var identityMember = MemberIdentityUser.CreateNew(
            createModel.Username,
            createModel.Email,
            memberType.Alias,
            createModel.IsApproved,
            createModel.InvariantName,
            createModel.Key);

        IdentityResult createResult = await _memberManager.CreateAsync(identityMember, createModel.Password);
        if (createResult.Succeeded is false)
        {
            return IdentityMemberCreationFailed(createResult, status);
        }

        IMember member = _memberService.GetByUsername(createModel.Username)
                          ?? throw new InvalidOperationException("Member creation succeeded, but member could not be found by username.");

        var updateRolesResult = await UpdateRoles(createModel.Roles, identityMember);
        if (updateRolesResult is false)
        {
            status.MemberEditingOperationStatus = MemberEditingOperationStatus.RoleAssignmentFailed;
            return Attempt.FailWithStatus(status, new MemberCreateResult { Content = member });
        }

        Attempt<MemberUpdateResult, ContentEditingOperationStatus> contentUpdateResult = await _memberContentEditingService.UpdateAsync(member, createModel, user.Key);

        status.MemberEditingOperationStatus = MemberEditingOperationStatus.Success;
        status.ContentEditingOperationStatus = contentUpdateResult.Status;

        return contentUpdateResult.Success
            ? Attempt.SucceedWithStatus(status, new MemberCreateResult { Content = member, ValidationResult = contentUpdateResult.Result.ValidationResult })
            : Attempt.FailWithStatus(status, new MemberCreateResult { Content = member });
    }

    public async Task<Attempt<MemberUpdateResult, MemberEditingStatus>> UpdateAsync(Guid key, MemberUpdateModel updateModel, IUser user)
    {
        var status = new MemberEditingStatus();

        IMember? member = _memberService.GetById(key);
        if (member is null)
        {
            status.ContentEditingOperationStatus = ContentEditingOperationStatus.NotFound;
            return Attempt.FailWithStatus(status, new MemberUpdateResult());
        }

        if (user.HasAccessToSensitiveData() is false)
        {
            // Handle sensitive data. Certain member properties (IsApproved, IsLockedOut) are subject to "sensitive data" rules.
            // The client won't have received these, so will always be false.
            // We should reset them back to their original values before proceeding with the update.
            updateModel.IsApproved = member.IsApproved;
            updateModel.IsLockedOut = member.IsLockedOut;
        }

        MemberIdentityUser? identityMember = await _memberManager.FindByIdAsync(member.Id.ToString());
        if (identityMember is null)
        {
            status.MemberEditingOperationStatus = MemberEditingOperationStatus.MemberNotFound;
            return Attempt.FailWithStatus(status, new MemberUpdateResult { Content = member });
        }

        MemberEditingOperationStatus validationStatus = await ValidateMemberDataAsync(updateModel, member.Key, updateModel.NewPassword);
        if (validationStatus is not MemberEditingOperationStatus.Success)
        {
            status.MemberEditingOperationStatus = validationStatus;
            return Attempt.FailWithStatus(status, new MemberUpdateResult { Content = member });
        }

        if (identityMember.IsLockedOut && updateModel.IsLockedOut is false)
        {
            var unlockResult = await UnlockMember(identityMember);
            if (unlockResult is false)
            {
                status.MemberEditingOperationStatus = MemberEditingOperationStatus.UnlockFailed;
                return Attempt.FailWithStatus(status, new MemberUpdateResult { Content = member });
            }
        }

        if (updateModel.IsTwoFactorEnabled is false)
        {
            var disableTwoFactorResult = await DisableTwoFactor(member);
            if (disableTwoFactorResult is false)
            {
                status.MemberEditingOperationStatus = MemberEditingOperationStatus.DisableTwoFactorFailed;
                return Attempt.FailWithStatus(status, new MemberUpdateResult { Content = member });
            }
        }

        if (updateModel.NewPassword.IsNullOrWhiteSpace() is false)
        {
            var changePasswordResult = await ChangePassword(member, updateModel.OldPassword, updateModel.NewPassword, user);
            if (changePasswordResult is false)
            {
                status.MemberEditingOperationStatus = MemberEditingOperationStatus.PasswordChangeFailed;
                return Attempt.FailWithStatus(status, new MemberUpdateResult { Content = member });
            }
        }

        var updateRolesResult = await UpdateRoles(updateModel.Roles, identityMember);
        if (updateRolesResult is false)
        {
            status.MemberEditingOperationStatus = MemberEditingOperationStatus.RoleAssignmentFailed;
            return Attempt.FailWithStatus(status, new MemberUpdateResult { Content = member });
        }

        member.IsLockedOut = updateModel.IsLockedOut;
        member.IsApproved = updateModel.IsApproved;
        member.Email = updateModel.Email;
        member.Username = updateModel.Username;

        Attempt<MemberUpdateResult, ContentEditingOperationStatus> contentUpdateResult = await _memberContentEditingService.UpdateAsync(member, updateModel, user.Key);

        status.MemberEditingOperationStatus = MemberEditingOperationStatus.Success;
        status.ContentEditingOperationStatus = contentUpdateResult.Status;

        return contentUpdateResult.Success
            ? Attempt.SucceedWithStatus(status, new MemberUpdateResult { Content = member, ValidationResult = contentUpdateResult.Result.ValidationResult })
            : Attempt.FailWithStatus(status, new MemberUpdateResult { Content = member });
    }

    public async Task<Attempt<IMember?, MemberEditingStatus>> DeleteAsync(Guid key, Guid userKey)
    {
        Attempt<IMember?, ContentEditingOperationStatus> contentDeleteResult = await _memberContentEditingService.DeleteAsync(key, userKey);
        return contentDeleteResult.Success
            ? Attempt.SucceedWithStatus(
                new MemberEditingStatus
                {
                    MemberEditingOperationStatus = MemberEditingOperationStatus.Success,
                    ContentEditingOperationStatus = contentDeleteResult.Status
                },
                contentDeleteResult.Result)
            : Attempt.FailWithStatus(
                new MemberEditingStatus
                {
                    MemberEditingOperationStatus = MemberEditingOperationStatus.Unknown,
                    ContentEditingOperationStatus = contentDeleteResult.Status
                },
                contentDeleteResult.Result);
    }

    private async Task<MemberEditingOperationStatus> ValidateMemberDataAsync(MemberEditingModelBase model, Guid? memberKey, string? password)
    {
        if (model.InvariantName.IsNullOrWhiteSpace())
        {
            return MemberEditingOperationStatus.InvalidName;
        }

        if (model.Username.IsNullOrWhiteSpace())
        {
            return MemberEditingOperationStatus.InvalidUsername;
        }

        // User names can only contain the configured allowed characters. This is validated by ASP.NET Identity on create
        // as the setting is applied to the BackOfficeIdentityOptions, but we need to check ourselves for updates.
        var allowedUserNameCharacters = _securitySettings.AllowedUserNameCharacters;
        if (model.Username.Any(c => allowedUserNameCharacters.Contains(c) == false))
        {
            return MemberEditingOperationStatus.InvalidUsername;
        }

        if (model.Email.IsEmail() is false)
        {
            return MemberEditingOperationStatus.InvalidEmail;
        }

        if (password is not null)
        {
            IdentityResult validatePassword = await _memberManager.ValidatePasswordAsync(password);
            if (validatePassword.Succeeded == false)
            {
                return MemberEditingOperationStatus.InvalidPassword;
            }
        }

        IMember? byUsername = _memberService.GetByUsername(model.Username);
        if (byUsername is not null && byUsername.Key != memberKey)
        {
            return MemberEditingOperationStatus.DuplicateUsername;
        }

        if (_securitySettings.MemberRequireUniqueEmail)
        {
            IMember? byEmail = _memberService.GetByEmail(model.Email);
            if (byEmail is not null && byEmail.Key != memberKey)
            {
                return MemberEditingOperationStatus.DuplicateEmail;
            }
        }

        return MemberEditingOperationStatus.Success;
    }

    private async Task<bool> UpdateRoles(IEnumerable<Guid>? roles, MemberIdentityUser identityMember)
    {
        // We have to convert the GUIDS to names here, as roles on a member are stored by name, not key.
        var memberGroups = new List<IMemberGroup>();
        foreach (Guid key in roles ?? Enumerable.Empty<Guid>())
        {

            IMemberGroup? group = await _memberGroupService.GetAsync(key);
            if (group is not null)
            {
                memberGroups.Add(group);
            }
        }

        // We're gonna look up the current roles now because the below code can cause
        // events to be raised and developers could be manually adding roles to members in
        // their handlers. If we don't look this up now there's a chance we'll just end up
        // removing the roles they've assigned.
        IEnumerable<string> currentRoles = (await _memberManager.GetRolesAsync(identityMember)).ToList();

        // find the ones to remove and remove them
        IEnumerable<string> memberGroupNames = memberGroups.Select(x => x.Name).WhereNotNull().ToArray();
        var rolesToRemove = currentRoles.Except(memberGroupNames).ToArray();

        // Now let's do the role provider stuff - now that we've saved the content item (that is important since
        // if we are changing the username, it must be persisted before looking up the member roles).
        if (rolesToRemove.Any())
        {
            IdentityResult identityResult = await _memberManager.RemoveFromRolesAsync(identityMember, rolesToRemove);
            if (!identityResult.Succeeded)
            {
                _logger.LogError("Could not remove roles from member: {errorMessage}", identityResult.Errors.ToErrorMessage());
                return false;
            }
        }

        // find the ones to add and add them
        var rolesToAdd = memberGroupNames.Except(currentRoles).ToArray();
        if (rolesToAdd.Any())
        {
            // add the ones submitted
            IdentityResult identityResult = await _memberManager.AddToRolesAsync(identityMember, rolesToAdd);
            if (!identityResult.Succeeded)
            {
                _logger.LogError("Could not add roles to member: {errorMessage}", identityResult.Errors.ToErrorMessage());
                return false;
            }
        }

        return true;
    }

    private static Attempt<MemberCreateResult, MemberEditingStatus> IdentityMemberCreationFailed(IdentityResult created, MemberEditingStatus status)
    {
        MemberEditingOperationStatus createStatus = MemberEditingOperationStatus.Unknown;
        foreach (IdentityError error in created.Errors)
        {
            switch (error.Code)
            {
                case nameof(IdentityErrorDescriber.InvalidUserName):
                    createStatus = MemberEditingOperationStatus.InvalidUsername;
                    break;
                case nameof(IdentityErrorDescriber.PasswordMismatch):
                case nameof(IdentityErrorDescriber.PasswordRequiresDigit):
                case nameof(IdentityErrorDescriber.PasswordRequiresLower):
                case nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric):
                case nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars):
                case nameof(IdentityErrorDescriber.PasswordRequiresUpper):
                case nameof(IdentityErrorDescriber.PasswordTooShort):
                    createStatus = MemberEditingOperationStatus.InvalidPassword;
                    break;
                case nameof(IdentityErrorDescriber.InvalidEmail):
                    createStatus = MemberEditingOperationStatus.InvalidEmail;
                    break;
                case nameof(IdentityErrorDescriber.DuplicateUserName):
                    createStatus = MemberEditingOperationStatus.DuplicateUsername;
                    break;
                case nameof(IdentityErrorDescriber.DuplicateEmail):
                    createStatus = MemberEditingOperationStatus.DuplicateEmail;
                    break;
                case MemberUserStore.CancelledIdentityErrorCode:
                    createStatus = MemberEditingOperationStatus.CancelledByNotificationHandler;
                    break;
            }

            if (createStatus is not MemberEditingOperationStatus.Unknown)
            {
                break;
            }
        }

        status.MemberEditingOperationStatus = createStatus;
        return Attempt.FailWithStatus(status, new MemberCreateResult());
    }

    private async Task<bool> UnlockMember(MemberIdentityUser identityMember)
    {
        // Handle unlocking with the member manager (takes care of other nuances)
        IdentityResult unlockResult = await _memberManager.SetLockoutEndDateAsync(identityMember, DateTimeOffset.Now.AddMinutes(-1));
        if (unlockResult.Succeeded is false)
        {
            _logger.LogError("Could not unlock member: {errorMessage}", unlockResult.Errors.ToErrorMessage());
        }

        return unlockResult.Succeeded;
    }

    private async Task<bool> DisableTwoFactor(IMember member)
    {
        IEnumerable<string> providers = await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(member.Key);
        foreach (var provider in providers)
        {
            var disableResult = await _twoFactorLoginService.DisableAsync(member.Key, provider);
            if (disableResult is false)
            {
                _logger.LogError("2FA provider \"{provider}\" could not disable member", provider);
                return false;
            }
        }

        return true;
    }

    private async Task<bool> ChangePassword(IMember member, string? oldPassword, string newPassword, IUser user)
    {
        var changingPasswordModel = new ChangingPasswordModel
        {
            Id = member.Id,
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        // change and persist the password
        Attempt<PasswordChangedModel?> passwordChangeResult =
            await _passwordChanger.ChangePasswordWithIdentityAsync(changingPasswordModel, _memberManager, user);

        if (passwordChangeResult.Success is false)
        {
            _logger.LogError("Could not change member password: {errorMessage}", passwordChangeResult.Result?.Error?.ErrorMessage ?? "no error details available");
            return false;
        }

        return true;
    }
}
