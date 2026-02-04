using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Installer;

/// <summary>
/// Provides functionality for performing a fresh installation of Umbraco CMS.
/// </summary>
public interface IInstallService
{
    /// <summary>
    /// Runs all the steps in the <see cref="NewInstallStepCollection"/>, installing Umbraco
    /// </summary>
    /// <param name="model">InstallData containing the required data used to install</param>
    /// <returns>
    /// An <see cref="Attempt{TResult,TStatus}"/> containing the <see cref="InstallationResult"/> on success,
    /// or an <see cref="InstallOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<InstallationResult?, InstallOperationStatus>> InstallAsync(InstallData model);
}
