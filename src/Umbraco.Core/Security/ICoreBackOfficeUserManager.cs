using System.Security.Principal;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

public interface ICoreBackOfficeUserManager
{
    Task<IdentityCreationResult> CreateAsync(UserCreateModel createModel);

    /// <summary>
    /// Creates a user for an invite. This means that the password will not be populated with
    /// </summary>
    /// <param name="createModel"></param>
    /// <returns></returns>
    Task<IdentityCreationResult> CreateForInvite(UserCreateModel createModel);

    Task<Attempt<string, UserOperationStatus>> GenerateEmailConfirmationTokenAsync(IUser user);

    Task<Attempt<string, UserOperationStatus>> GeneratePasswordResetTokenAsync(IUser user);

    Task<Attempt<UserUnlockResult, UserOperationStatus>> UnlockUser(IUser user);

    Task<Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus>> GetLoginsAsync(IUser user);

    Task<bool> IsEmailConfirmationTokenValidAsync(IUser user, string token);

    Task<bool> IsResetPasswordTokenValidAsync(IUser user, string token);

    void NotifyForgotPasswordRequested(IPrincipal user, string toString);

    public string GeneratePassword();
}
