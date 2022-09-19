using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Web.Common.Installer;

public class SignInUserStep : IInstallStep
{
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;

    public SignInUserStep(
        IBackOfficeSignInManager backOfficeSignInManager,
        IBackOfficeUserManager backOfficeUserManager)
    {
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
    }

    public async Task ExecuteAsync(InstallData model)
    {
        BackOfficeIdentityUser identityUser = await _backOfficeUserManager.FindByIdAsync(Constants.Security.SuperUserIdAsString);
        await _backOfficeSignInManager.SignInAsync(identityUser, false);
    }

    public Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(true);
}
