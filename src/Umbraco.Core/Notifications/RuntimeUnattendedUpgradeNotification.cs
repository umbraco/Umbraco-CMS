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
    public enum UpgradeResult
    {
        NotRequired = 0,
        HasErrors = 1,
        CoreUpgradeComplete = 100,
        PackageMigrationComplete = 101,
    }

    /// <summary>
    ///     Gets/sets the result of the unattended upgrade
    /// </summary>
    public UpgradeResult UnattendedUpgradeResult { get; set; } = UpgradeResult.NotRequired;
}
