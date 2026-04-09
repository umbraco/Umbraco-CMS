namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the database configuration data provided during installation.
/// </summary>
public class DatabaseInstallData
{
    /// <summary>
    ///     Gets or sets the unique identifier of the selected database configuration.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the database provider name.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    ///     Gets or sets the database server address.
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    ///     Gets or sets the database name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the database username for authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     Gets or sets the database password for authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to use integrated authentication.
    /// </summary>
    public bool UseIntegratedAuthentication { get; set; }

    /// <summary>
    ///     Gets or sets the full connection string, if provided directly.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to trust the server certificate.
    /// </summary>
    public bool TrustServerCertificate { get; set; }
}
