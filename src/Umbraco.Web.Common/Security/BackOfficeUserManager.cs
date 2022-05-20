using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public class BackOfficeUserManager : UmbracoUserManager<BackOfficeIdentityUser, UserPasswordConfigurationSettings>,
    IBackOfficeUserManager
{
    private readonly IBackOfficeUserPasswordChecker _backOfficeUserPasswordChecker;
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
        IBackOfficeUserPasswordChecker backOfficeUserPasswordChecker)
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
    }

    /// <summary>
    ///     Override to check the user approval value as well as the user lock out date, by default this only checks the user's
    ///     locked out date
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>True if the user is locked out, else false</returns>
    /// <remarks>
    ///     In the ASP.NET Identity world, there is only one value for being locked out, in Umbraco we have 2 so when checking
    ///     this for Umbraco we need to check both values
    /// </remarks>
    public override async Task<bool> IsLockedOutAsync(BackOfficeIdentityUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (user.IsApproved == false)
        {
            return true;
        }

        return await base.IsLockedOutAsync(user);
    }

    public override async Task<IdentityResult> AccessFailedAsync(BackOfficeIdentityUser user)
    {
        IdentityResult result = await base.AccessFailedAsync(user);

        // Slightly confusing: this will return a Success if we successfully update the AccessFailed count
        if (result.Succeeded)
        {
            NotifyLoginFailed(_httpContextAccessor.HttpContext?.User, user.Id);
        }

        return result;
    }

    public override async Task<IdentityResult> ChangePasswordWithResetAsync(string userId, string token, string? newPassword)
    {
        IdentityResult result = await base.ChangePasswordWithResetAsync(userId, token, newPassword);
        if (result.Succeeded)
        {
            NotifyPasswordReset(_httpContextAccessor.HttpContext?.User, userId);
        }

        return result;
    }

    public override async Task<IdentityResult> ChangePasswordAsync(BackOfficeIdentityUser user, string? currentPassword, string? newPassword)
    {
        IdentityResult result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            NotifyPasswordChanged(_httpContextAccessor.HttpContext?.User, user.Id);
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
            NotifyAccountLocked(_httpContextAccessor.HttpContext?.User, user.Id);
        }
        else
        {
            NotifyAccountUnlocked(_httpContextAccessor.HttpContext?.User, user.Id);

            // Resets the login attempt fails back to 0 when unlock is clicked
            await ResetAccessFailedCountAsync(user);
        }

        return result;
    }

    public override async Task<IdentityResult> ResetAccessFailedCountAsync(BackOfficeIdentityUser user)
    {
        IdentityResult result = await base.ResetAccessFailedCountAsync(user);

        // notify now that it's reset
        NotifyResetAccessFailedCount(_httpContextAccessor.HttpContext?.User, user.Id);

        return result;
    }

    public void NotifyForgotPasswordRequested(IPrincipal currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserForgotPasswordRequestedNotification(ip, userId, currentUserId));

    public void NotifyForgotPasswordChanged(IPrincipal currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserForgotPasswordChangedNotification(ip, userId, currentUserId));

    public SignOutSuccessResult NotifyLogoutSuccess(IPrincipal currentUser, string? userId)
    {
        UserLogoutSuccessNotification notification = Notify(
            currentUser,
            (currentUserId, ip) => new UserLogoutSuccessNotification(ip, userId, currentUserId));

        return new SignOutSuccessResult { SignOutRedirectUrl = notification.SignOutRedirectUrl };
    }

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

    public void NotifyAccountUnlocked(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserUnlockedNotification(ip, userId, currentUserId));

    public void NotifyLoginFailed(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserLoginFailedNotification(ip, userId, currentUserId));

    public void NotifyLoginRequiresVerification(IPrincipal currentUser, string? userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserLoginRequiresVerificationNotification(ip, userId, currentUserId));

    public void NotifyLoginSuccess(IPrincipal currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserLoginSuccessNotification(ip, userId, currentUserId));

    public void NotifyPasswordChanged(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserPasswordChangedNotification(ip, userId, currentUserId));

    public void NotifyPasswordReset(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserPasswordResetNotification(ip, userId, currentUserId));

    public void NotifyResetAccessFailedCount(IPrincipal? currentUser, string userId) => Notify(
        currentUser,
        (currentUserId, ip) => new UserResetAccessFailedCountNotification(ip, userId, currentUserId));

    private T Notify<T>(IPrincipal? currentUser, Func<string, string, T> createNotification)
        where T : INotification
    {
        var currentUserId = GetCurrentUserId(currentUser);
        var ip = IpResolver.GetCurrentRequestIpAddress();

        T notification = createNotification(currentUserId, ip);
        _eventAggregator.Publish(notification);
        return notification;
    }
}
