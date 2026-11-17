using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public class BackOfficeUserManager : UmbracoUserManager<BackOfficeIdentityUser, UserPasswordConfigurationSettings>,
    IBackOfficeUserManager,
    ICoreBackOfficeUserManager
{
    private readonly IBackOfficeUserPasswordChecker _backOfficeUserPasswordChecker;
    private readonly GlobalSettings _globalSettings;
    private readonly IEventAggregator _eventAggregator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BackOfficeUserManager(
        IIpResolver ipResolver,
        IUserStore<BackOfficeIdentityUser> store,
        IOptions<BackOfficeIdentityOptions> optionsAccessor,
        IPasswordHasher<BackOfficeIdentityUser> passwordHasher,
        IEnumerable<IUserValidator<BackOfficeIdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<BackOfficeIdentityUser>> passwordValidators,
        BackOfficeErrorDescriber errors,
        IServiceProvider services,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserManager<BackOfficeIdentityUser>> logger,
        IOptions<UserPasswordConfigurationSettings> passwordConfiguration,
        IEventAggregator eventAggregator,
        IBackOfficeUserPasswordChecker backOfficeUserPasswordChecker,
        IOptions<GlobalSettings> globalSettings)
        : base(
            ipResolver,
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            errors,
            services,
            logger,
            passwordConfiguration)
    {
        _httpContextAccessor = httpContextAccessor;
        _eventAggregator = eventAggregator;
        _backOfficeUserPasswordChecker = backOfficeUserPasswordChecker;
        _globalSettings = globalSettings.Value;
    }

    public override async Task<IdentityResult> AccessFailedAsync(BackOfficeIdentityUser user)
    {
        IdentityResult result = await base.AccessFailedAsync(user);

        // Slightly confusing: this will return a Success if we successfully update the AccessFailed count
        if (result.Succeeded)
        {
            await NotifyLoginFailedAsync(_httpContextAccessor.HttpContext?.User, user.Id);
        }

        return result;
    }

    public override async Task<IdentityResult> ChangePasswordWithResetAsync(string userId, string token, string newPassword)
    {
        IdentityResult result = await base.ChangePasswordWithResetAsync(userId, token, newPassword);
        if (result.Succeeded)
        {
            await NotifyPasswordResetAsync(_httpContextAccessor.HttpContext?.User, userId);
        }

        return result;
    }

    public override async Task<IdentityResult> ChangePasswordAsync(BackOfficeIdentityUser user, string currentPassword, string newPassword)
    {
        IdentityResult result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            await NotifyPasswordChangedAsync(_httpContextAccessor.HttpContext?.User, user.Id);
        }

        return result;
    }

    public override async Task<IdentityResult> SetLockoutEndDateAsync(
        BackOfficeIdentityUser user,
        DateTimeOffset? lockoutEnd)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        IdentityResult result = await base.SetLockoutEndDateAsync(user, lockoutEnd);

        // The way we unlock is by setting the lockoutEnd date to the current datetime
        if (result.Succeeded && lockoutEnd > DateTimeOffset.UtcNow)
        {
            await NotifyAccountLockedAsync(_httpContextAccessor.HttpContext?.User, user.Id);
        }
        else
        {
            await NotifyAccountUnlockedAsync(_httpContextAccessor.HttpContext?.User, user.Id);

            // Resets the login attempt fails back to 0 when unlock is clicked
            await ResetAccessFailedCountAsync(user);
        }

        return result;
    }

    public async Task<Attempt<UserUnlockResult, UserOperationStatus>> UnlockUser(IUser user)
    {
        BackOfficeIdentityUser? identityUser = await FindByIdAsync(user.Id.ToString());

        if (identityUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, new UserUnlockResult());
        }

        IdentityResult result = await SetLockoutEndDateAsync(identityUser, DateTimeOffset.Now.AddMinutes(-1));

        return result.Succeeded
            ? Attempt.SucceedWithStatus(UserOperationStatus.Success, new UserUnlockResult())
            : Attempt.FailWithStatus(UserOperationStatus.UnknownFailure, new UserUnlockResult { Error = new ValidationResult(result.Errors.ToErrorMessage()) });
    }

    public override async Task<IdentityResult> ResetAccessFailedCountAsync(BackOfficeIdentityUser user)
    {
        IdentityResult result = await base.ResetAccessFailedCountAsync(user);

        // notify now that it's reset
        await NotifyResetAccessFailedCountAsync(_httpContextAccessor.HttpContext?.User, user.Id);

        return result;
    }

    [Obsolete("Use NotifyForgotPasswordRequestedAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyForgotPasswordRequested(IPrincipal currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserForgotPasswordRequestedNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyForgotPasswordChangedAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyForgotPasswordChanged(IPrincipal currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserForgotPasswordChangedNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyLogoutSuccessAsync instead. Scheduled for removal in Umbraco 21.")]
    public SignOutSuccessResult NotifyLogoutSuccess(IPrincipal currentUser, string? userId)
    {
        UserLogoutSuccessNotification notification = Notify(
            currentUser,
            (currentUserId, ip) => new UserLogoutSuccessNotification(ip, userId, currentUserId));

        return new SignOutSuccessResult { SignOutRedirectUrl = notification.SignOutRedirectUrl };
    }

    [Obsolete("Use NotifyAccountLockedAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyAccountLocked(IPrincipal? currentUser, string? userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserLockedNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Override to allow checking the password via the <see cref="IBackOfficeUserPasswordChecker" /> if one is configured
    /// </summary>
    /// <param name="store"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(
        IUserPasswordStore<BackOfficeIdentityUser> store,
        BackOfficeIdentityUser user,
        string password)
    {
        if (user.HasIdentity == false)
        {
            return PasswordVerificationResult.Failed;
        }

        BackOfficeUserPasswordCheckerResult result =
            await _backOfficeUserPasswordChecker.CheckPasswordAsync(user, password);

        // if the result indicates to not fallback to the default, then return true if the credentials are valid
        if (result != BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker)
        {
            return result == BackOfficeUserPasswordCheckerResult.ValidCredentials
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

        return await base.VerifyPasswordAsync(store, user, password);
    }

    private string GetCurrentUserId(IPrincipal? currentUser)
    {
        ClaimsIdentity? umbIdentity = currentUser?.GetUmbracoIdentity();
        var currentUserId = umbIdentity?.GetUserId<string>() ?? Core.Constants.Security.SuperUserIdAsString;
        return currentUserId;
    }

    [Obsolete("Use NotifyAccountUnlockedAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyAccountUnlocked(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserUnlockedNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyLoginFailedAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyLoginFailed(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserLoginFailedNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyLoginRequiresVerificationAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyLoginRequiresVerification(IPrincipal currentUser, string? userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserLoginRequiresVerificationNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyLoginSuccessAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyLoginSuccess(IPrincipal currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserLoginSuccessNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyPasswordChangedAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyPasswordChanged(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserPasswordChangedNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyPasswordResetAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyPasswordReset(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserPasswordResetNotification(ip, userId, currentUserId));

    [Obsolete("Use NotifyResetAccessFailedCountAsync instead. Scheduled for removal in Umbraco 21.")]
    public void NotifyResetAccessFailedCount(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserResetAccessFailedCountNotification(ip, userId, currentUserId));

    /// <inheritdoc />
    public Task NotifyForgotPasswordRequestedAsync(IPrincipal currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserForgotPasswordRequestedNotification(ip, userId, currentUserId));

    /// <inheritdoc />
    public Task NotifyForgotPasswordChangedAsync(IPrincipal currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserForgotPasswordChangedNotification(ip, userId, currentUserId));

    /// <inheritdoc />
    public async Task<SignOutSuccessResult> NotifyLogoutSuccessAsync(IPrincipal currentUser, string? userId)
    {
        UserLogoutSuccessNotification notification = await NotifyAsync(
            currentUser,
            (currentUserId, ip) => new UserLogoutSuccessNotification(ip, userId, currentUserId));

        return new SignOutSuccessResult { SignOutRedirectUrl = notification.SignOutRedirectUrl };
    }

    /// <summary>
    ///     Publishes a <see cref="UserLockedNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user that was locked.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyAccountLockedAsync(IPrincipal? currentUser, string? userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserLockedNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Publishes a <see cref="UserUnlockedNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user that was unlocked.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyAccountUnlockedAsync(IPrincipal? currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserUnlockedNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Publishes a <see cref="UserLoginFailedNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user whose login failed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyLoginFailedAsync(IPrincipal? currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserLoginFailedNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Publishes a <see cref="UserLoginRequiresVerificationNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user whose login requires verification.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyLoginRequiresVerificationAsync(IPrincipal currentUser, string? userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserLoginRequiresVerificationNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Publishes a <see cref="UserLoginSuccessNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user that logged in.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyLoginSuccessAsync(IPrincipal currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserLoginSuccessNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Publishes a <see cref="UserPasswordChangedNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user whose password was changed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyPasswordChangedAsync(IPrincipal? currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserPasswordChangedNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Publishes a <see cref="UserPasswordResetNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user whose password was reset.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyPasswordResetAsync(IPrincipal? currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserPasswordResetNotification(ip, userId, currentUserId));

    /// <summary>
    ///     Publishes a <see cref="UserResetAccessFailedCountNotification" /> for the specified user.
    /// </summary>
    /// <param name="currentUser">The principal of the user performing the action.</param>
    /// <param name="userId">The identifier of the user whose failed access count was reset.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyResetAccessFailedCountAsync(IPrincipal? currentUser, string userId) => NotifyAsync(
        currentUser,
        (currentUserId, ip) => new UserResetAccessFailedCountNotification(ip, userId, currentUserId));

    private T Notify<T>(IPrincipal? currentUser, Func<string, string, T> createNotification)
        where T : INotification
    {
        var currentUserId = GetCurrentUserId(currentUser);
        var ip = IpResolver.GetCurrentRequestIpAddress();

        T notification = createNotification(currentUserId, ip);
#pragma warning disable CS0618 // Type or member is obsolete
        _eventAggregator.Publish(notification);
#pragma warning restore CS0618 // Type or member is obsolete
        return notification;
    }

    private async Task<T> NotifyAsync<T>(IPrincipal? currentUser, Func<string, string, T> createNotification)
        where T : INotification
    {
        var currentUserId = GetCurrentUserId(currentUser);
        var ip = IpResolver.GetCurrentRequestIpAddress();

        T notification = createNotification(currentUserId, ip);
        await _eventAggregator.PublishAsync(notification);
        return notification;
    }

    public async Task<IdentityCreationResult> CreateForInvite(UserCreateModel createModel)
    {
        var identityUser = BackOfficeIdentityUser.CreateNew(
            _globalSettings,
            createModel.UserName,
            createModel.Email,
            _globalSettings.DefaultUILanguage);

        identityUser.Name = createModel.Name;

        IdentityResult created = await CreateAsync(identityUser);

        return created.Succeeded
            ? new IdentityCreationResult { Succeded = true }
            : IdentityCreationResult.Fail(created.Errors.ToErrorMessage());
    }

    public async Task<IdentityCreationResult> CreateAsync(UserCreateModel createModel)
    {
        var identityUser = BackOfficeIdentityUser.CreateNew(
            _globalSettings,
            createModel.UserName,
            createModel.Email,
            _globalSettings.DefaultUILanguage,
            createModel.Name,
            createModel.Id,
            createModel.Kind);

        IdentityResult created = await CreateAsync(identityUser);

        if (created.Succeeded is false)
        {
            if (created.Errors.Any(e => e.Code == nameof(UserOperationStatus.CancelledByNotification)))
            {
                return IdentityCreationResult.Cancel();
            }

            return IdentityCreationResult.Fail(created.Errors.ToErrorMessage());
        }

        var password = GeneratePassword();

        IdentityResult passwordAdded = await AddPasswordAsync(identityUser, password);
        if (passwordAdded.Succeeded is false)
        {
            return IdentityCreationResult.Fail(passwordAdded.Errors.ToErrorMessage());
        }

        return new IdentityCreationResult { Succeded = true, InitialPassword = password };
    }

    public async Task<Attempt<string, UserOperationStatus>> GeneratePasswordResetTokenAsync(IUser user)
    {
        BackOfficeIdentityUser? identityUser = await FindByIdAsync(user.Id.ToString());

        if (identityUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, string.Empty);
        }

        var token = await GeneratePasswordResetTokenAsync(identityUser);

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, token);
    }
    public async Task<Attempt<string, UserOperationStatus>> GenerateEmailConfirmationTokenAsync(IUser user)
    {
        BackOfficeIdentityUser? identityUser = await FindByIdAsync(user.Id.ToString());

        if (identityUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, string.Empty);
        }

        var token = await GenerateEmailConfirmationTokenAsync(identityUser);

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, token);
    }

    public async Task<Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus>> GetLoginsAsync(IUser user)
    {
        BackOfficeIdentityUser? identityUser = await FindByIdAsync(user.Id.ToString());
        if (identityUser is null)
        {
            return Attempt.FailWithStatus<ICollection<IIdentityUserLogin>, UserOperationStatus>(UserOperationStatus.UserNotFound, Array.Empty<IIdentityUserLogin>());
        }

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, identityUser.Logins);
    }

    public async Task<bool> IsEmailConfirmationTokenValidAsync(IUser user, string token)
    {
        BackOfficeIdentityUser? identityUser = await FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture));

        if (identityUser != null && await VerifyUserTokenAsync(identityUser, Options.Tokens.EmailConfirmationTokenProvider, ConfirmEmailTokenPurpose, token).ConfigureAwait(false))
        {
            return true;
        }

        return false;
    }

    public async Task<bool> IsResetPasswordTokenValidAsync(IUser user, string token)
    {
        BackOfficeIdentityUser? identityUser = await FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture));

        if (identityUser != null && await VerifyUserTokenAsync(identityUser, Options.Tokens.PasswordResetTokenProvider, ResetPasswordTokenPurpose, token).ConfigureAwait(false))
        {
            return true;
        }

        return false;
    }
}
