using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents the configuration settings required for database setup during installation.
/// </summary>
public class DatabaseSettingsPresentationModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the database settings.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the order in which the database settings are presented in the installer UI.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the user-friendly name associated with the database settings.
    /// </summary>
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default name to be used for the database during installation.
    /// </summary>
    [Required]
    public string DefaultDatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the database provider.
    /// </summary>
    [Required]
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the database has been configured.
    /// </summary>
    public bool IsConfigured { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a server name is required for the database connection.
    /// </summary>
    public bool RequiresServer { get; set; }

    /// <summary>Gets or sets the placeholder text for the database server input field.</summary>
    [Required]
    public string ServerPlaceholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether database credentials are required.
    /// </summary>
    public bool RequiresCredentials { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the database supports integrated authentication (such as Windows Authentication).
    /// </summary>
    public bool SupportsIntegratedAuthentication { get; set; }

    /// <summary>Gets or sets a value indicating whether the database connection supports trusting the server certificate.</summary>
    public bool SupportsTrustServerCertificate { get; set; }

    /// <summary>
    /// Indicates whether a test of the database connection is required.
    /// </summary>
    public bool RequiresConnectionTest { get; set; }
}
