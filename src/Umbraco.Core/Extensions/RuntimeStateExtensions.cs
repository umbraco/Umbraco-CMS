using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class RuntimeStateExtensions
{
    /// <summary>
    ///     Returns true if the installer is enabled based on the current runtime state
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool EnableInstaller(this IRuntimeState state)
        => state.Level == RuntimeLevel.Install || state.Level == RuntimeLevel.Upgrade;

    // TODO: If we want to enable the installer for package migrations, but IMO i think we should do migrations in the back office
    // if they are not unattended.
    // => state.Level == RuntimeLevel.Install || state.Level == RuntimeLevel.Upgrade || state.Level == RuntimeLevel.PackageMigrations;

    /// <summary>
    ///     Returns true if Umbraco <see cref="IRuntimeState" /> is greater than <see cref="RuntimeLevel.BootFailed" />
    /// </summary>
    public static bool UmbracoCanBoot(this IRuntimeState state) => state.Level > RuntimeLevel.BootFailed;

    /// <summary>
    ///     Returns true if the runtime state indicates that unattended boot logic should execute
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool RunUnattendedBootLogic(this IRuntimeState state)
        => (state.Reason == RuntimeLevelReason.UpgradeMigrations ||
            state.Reason == RuntimeLevelReason.UpgradePackageMigrations)
           && state.Level == RuntimeLevel.Run;
}
