using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents the data required to perform a database installation during the Umbraco setup process.
/// </summary>
public class DatabaseInstallRequestModel
{
    /// <summary>
    /// Gets or sets the unique identifier for this database installation process.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the database provider.
    /// </summary>
    [Required]
    public string? ProviderName { get; set; }

    /// <summary>
    /// Gets or sets the database server name or address.
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the username for the database connection.</summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for the database connection.
    /// </summary>
    [PasswordPropertyText]
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use integrated authentication for the database connection.
    /// </summary>
    public bool UseIntegratedAuthentication { get; set; }

    /// <summary>Gets or sets the connection string for the database installation.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Gets or sets a value indicating whether to trust the server certificate when connecting to the database.</summary>
    public bool TrustServerCertificate { get; set; }
}
