
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;


namespace Umbraco.Cms.Web.Common.Installer;

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
        BackOfficeIdentityUser? identityUser = await _backOfficeUserManager.FindByIdAsync(Core.Constants.Security.SuperUserIdAsString);

        if (identityUser is not null)
        {
            await _backOfficeSignInManager.SignInAsync(identityUser, false);
        }
    }

    public Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(true);
}
