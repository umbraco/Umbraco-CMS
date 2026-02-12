// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when runtime pre-migrations need to be executed during upgrade.
/// </summary>
/// <remarks>
///     Handlers can process this notification to perform pre-migration upgrade steps
///     and set the <see cref="UpgradeResult"/> to indicate the outcome.
/// </remarks>
public class RuntimePremigrationsUpgradeNotification : INotification
{
    /// <summary>
    ///     Represents the possible results of a pre-migration upgrade operation.
    /// </summary>
    public enum PremigrationUpgradeResult
    {
        /// <summary>
        ///     No upgrade was required.
        /// </summary>
        NotRequired = 0,

        /// <summary>
        ///     The upgrade encountered errors.
        /// </summary>
        HasErrors = 1,

        /// <summary>
        ///     The core upgrade completed successfully.
        /// </summary>
        CoreUpgradeComplete = 100,
    }

    /// <summary>
    ///     Gets or sets the result of the upgrade.
    /// </summary>
    public PremigrationUpgradeResult UpgradeResult { get; set; } = PremigrationUpgradeResult.NotRequired;
}
