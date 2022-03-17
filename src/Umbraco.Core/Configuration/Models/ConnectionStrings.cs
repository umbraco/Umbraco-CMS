using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions("ConnectionStrings")]
public class ConnectionStrings
{
    private string _connectionString;

    /// <summary>
    /// The default provider name when not present in configuration.
    /// </summary>
    public const string DefaultProviderName = "Microsoft.Data.SqlClient";

    /// <summary>
    /// The DataDirectory placeholder.
    /// </summary>
    public const string DataDirectoryPlaceholder = "|DataDirectory|";

    /// <summary>
    /// The postfix used to identify a connection strings provider setting.
    /// </summary>
    public const string ProviderNamePostfix = "_ProviderName";

    public string Name { get; set; }

    public string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value.ReplaceDataDirectoryPlaceholder();
    }

    public string? ProviderName { get; set; } = DefaultProviderName;
}
