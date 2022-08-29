namespace Umbraco.Cms.Core.Persistence;

public enum DatabaseState
{
    Ok,
    CannotConnect,
    NotInstalled,
    NeedsUpgrade,
    NeedsPackageMigration,
    NotConfigured
}
