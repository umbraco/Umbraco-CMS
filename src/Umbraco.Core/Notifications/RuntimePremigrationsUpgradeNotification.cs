namespace Umbraco.Cms.Core.Notifications;

public class RuntimePremigrationsUpgradeNotification : INotification
{
    public enum PremigrationUpgradeResult
    {
        NotRequired = 0,
        HasErrors = 1,
        CoreUpgradeComplete = 100,
    }

    /// <summary>
    ///     Gets/sets the result of the upgrade
    /// </summary>
    public PremigrationUpgradeResult UpgradeResult { get; set; } = PremigrationUpgradeResult.NotRequired;
}
