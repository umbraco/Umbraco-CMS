// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Used to notify when the core runtime can do an unattended upgrade.
/// </summary>
/// <remarks>
///     It is entirely up to the handler to determine if an unattended upgrade should occur and
///     to perform the logic.
/// </remarks>
public class RuntimeUnattendedUpgradeNotification : INotification
{
    /// <summary>
    ///     Represents the possible results of an unattended upgrade operation.
    /// </summary>
    public enum UpgradeResult
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

        /// <summary>
        ///     The package migration completed successfully.
        /// </summary>
        PackageMigrationComplete = 101,
    }

    /// <summary>
    ///     Gets/sets the result of the unattended upgrade
    /// </summary>
    public UpgradeResult UnattendedUpgradeResult { get; set; } = UpgradeResult.NotRequired;
}
