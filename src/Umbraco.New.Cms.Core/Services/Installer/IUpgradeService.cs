using Umbraco.New.Cms.Core.Installer;

namespace Umbraco.New.Cms.Core.Services.Installer;

public interface IUpgradeService
{
    /// <summary>
    /// Runs all the steps in the <see cref="UpgradeStepCollection"/>, upgrading Umbraco.
    /// </summary>
    Task Upgrade();
}
