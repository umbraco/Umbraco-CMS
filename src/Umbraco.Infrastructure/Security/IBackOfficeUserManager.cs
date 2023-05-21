using System.Security.Principal;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The user manager for the back office
/// </summary>
public interface IBackOfficeUserManager : IUmbracoUserManager<BackOfficeIdentityUser>
{
    void NotifyForgotPasswordRequested(IPrincipal currentUser, string userId);

    void NotifyForgotPasswordChanged(IPrincipal currentUser, string userId);

    SignOutSuccessResult NotifyLogoutSuccess(IPrincipal currentUser, string? userId);
}
