using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Installer;

public interface IUpgradeService
{
    /// <summary>
    /// Runs all the steps in the <see cref="UpgradeStepCollection"/>, upgrading Umbraco.
    /// </summary>
    Task<Attempt<InstallationResult?, UpgradeOperationStatus>> UpgradeAsync();
}
