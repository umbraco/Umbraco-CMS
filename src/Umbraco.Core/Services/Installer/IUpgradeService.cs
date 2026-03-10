using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Installer;

/// <summary>
/// Provides functionality for upgrading an existing Umbraco CMS installation to a newer version.
/// </summary>
public interface IUpgradeService
{
    /// <summary>
    /// Runs all the steps in the <see cref="UpgradeStepCollection"/>, upgrading Umbraco.
    /// </summary>
    /// <returns>
    /// An <see cref="Attempt{TResult,TStatus}"/> containing the <see cref="InstallationResult"/> on success,
    /// or an <see cref="UpgradeOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<InstallationResult?, UpgradeOperationStatus>> UpgradeAsync();
}
