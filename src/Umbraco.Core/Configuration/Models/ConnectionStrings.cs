using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Represents a single connection string.
/// </summary>
public class ConnectionStrings // TODO: Rename to [Umbraco]ConnectionString (since v10 this only contains a single connection string)
{
    /// <summary>
    ///     The default provider name when not present in configuration.
    /// </summary>
    public const string DefaultProviderName = "Microsoft.Data.SqlClient";

    /// <summary>
    ///     The DataDirectory placeholder.
    /// </summary>
    public const string DataDirectoryPlaceholder = ConfigurationExtensions.DataDirectoryPlaceholder;

    /// <summary>
    ///     The postfix used to identify a connection strings provider setting.
    /// </summary>
    public const string ProviderNamePostfix = ConfigurationExtensions.ProviderNamePostfix;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [Obsolete("This property will be removed in Umbraco 12, because this class is now using named options.")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    /// <value>
    /// The connection string.
    /// </value>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the provider.
    /// </summary>
    /// <value>
    /// The name of the provider.
    /// </value>
    public string? ProviderName { get; set; }
}
