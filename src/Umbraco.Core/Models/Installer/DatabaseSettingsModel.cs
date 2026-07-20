namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the configuration settings for a database provider during installation.
/// </summary>
public class DatabaseSettingsModel
{
    /// <summary>
    ///     Gets or sets the unique identifier for this database configuration.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the sort order for displaying this database option.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the database provider.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the default database name to use.
    /// </summary>
    public string DefaultDatabaseName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the database provider.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the database is already configured.
    /// </summary>
    public bool IsConfigured { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the database requires a server address.
    /// </summary>
    public bool RequiresServer { get; set; }

    /// <summary>
    ///     Gets or sets the placeholder text for the server input field.
    /// </summary>
    public string ServerPlaceholder { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the database requires credentials.
    /// </summary>
    public bool RequiresCredentials { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the database supports integrated authentication.
    /// </summary>
    public bool SupportsIntegratedAuthentication { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the database supports the trust server certificate option.
    /// </summary>
    public bool SupportsTrustServerCertificate { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the database requires a connection test before proceeding.
    /// </summary>
    public bool RequiresConnectionTest { get; set; }
}
