using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Services.Installer;

public interface IInstallService
{
    /// <summary>
    /// Runs all the steps in the <see cref="NewInstallStepCollection"/>, installing Umbraco
    /// </summary>
    /// <param name="model">InstallData containing the required data used to install</param>
    /// <returns></returns>
    Task Install(InstallData model);
}
