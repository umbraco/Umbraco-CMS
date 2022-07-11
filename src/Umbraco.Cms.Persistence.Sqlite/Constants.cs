namespace Umbraco.Cms.Persistence.Sqlite;

/// <summary>
///     Constants related to SQLite.
/// </summary>
public static class Constants
{
    /// <summary>
    ///     SQLite provider name.
    /// </summary>
    public const string ProviderName = "Microsoft.Data.Sqlite";

    [Obsolete("This will be removed in Umbraco 12. Use Constants.ProviderName instead")]
    public const string ProviderNameLegacy = "Microsoft.Data.SQLite";
}
