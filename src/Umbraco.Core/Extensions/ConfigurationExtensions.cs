using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions;

/// <summary>
/// Extensions for <see cref="IConfiguration" />.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// The DataDirectory name.
    /// </summary>
    internal const string DataDirectoryName = "DataDirectory";

    /// <summary>
    /// The DataDirectory placeholder.
    /// </summary>
    internal const string DataDirectoryPlaceholder = "|DataDirectory|";

    /// <summary>
    /// The postfix used to identify a connection string provider setting.
    /// </summary>
    internal const string ProviderNamePostfix = "_ProviderName";

    /// <summary>
    /// Gets the provider name for the connection string name (shorthand for <c>GetSection("ConnectionStrings")[name + "_ProviderName"]</c>).
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="name">The connection string key.</param>
    /// <returns>
    /// The provider name.
    /// </returns>
    /// <remarks>
    /// This uses the same convention as the <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#connection-string-prefixes">Configuration API for connection string environment variables</a>.
    /// </remarks>
    public static string? GetConnectionStringProviderName(this IConfiguration configuration, string name)
        => configuration.GetConnectionString(name + ProviderNamePostfix);

    /// <summary>
    /// Gets the Umbraco connection string (shorthand for <c>GetSection("ConnectionStrings")[name]</c> and replacing the <c>|DataDirectory|</c> placeholder).
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="name">The connection string key.</param>
    /// <returns>
    /// The Umbraco connection string.
    /// </returns>
    public static string? GetUmbracoConnectionString(this IConfiguration configuration, string name = Constants.System.UmbracoConnectionName)
        => configuration.GetUmbracoConnectionString(name, out _);

    /// <summary>
    /// Gets the Umbraco connection string and provider name (shorthand for <c>GetSection("ConnectionStrings")[Constants.System.UmbracoConnectionName]</c> and replacing the <c>|DataDirectory|</c> placeholder).
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="providerName">The provider name.</param>
    /// <returns>
    /// The Umbraco connection string.
    /// </returns>
    public static string? GetUmbracoConnectionString(this IConfiguration configuration, out string? providerName)
        => configuration.GetUmbracoConnectionString(Constants.System.UmbracoConnectionName, out providerName);

    /// <summary>
    /// Gets the Umbraco connection string and provider name (shorthand for <c>GetSection("ConnectionStrings")[name]</c> and replacing the <c>|DataDirectory|</c> placeholder).
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="name">The name.</param>
    /// <param name="providerName">The provider name.</param>
    /// <returns>
    /// The Umbraco connection string.
    /// </returns>
    public static string? GetUmbracoConnectionString(this IConfiguration configuration, string name, out string? providerName)
    {
        string? connectionString = configuration.GetConnectionString(name);
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Replace data directory
            string? dataDirectory = AppDomain.CurrentDomain.GetData(DataDirectoryName)?.ToString();
            if (!string.IsNullOrEmpty(dataDirectory))
            {
                connectionString = connectionString.Replace(DataDirectoryPlaceholder, dataDirectory);
            }

            // Get provider name
            providerName = configuration.GetConnectionStringProviderName(name);
        }
        else
        {
            providerName = null;
        }

        return connectionString;
    }

    /// <summary>
    /// Gets the Umbraco runtime mode.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>
    /// The Umbraco runtime mode.
    /// </returns>
    public static RuntimeMode GetRuntimeMode(this IConfiguration configuration)
        => configuration.GetValue<RuntimeMode>(Constants.Configuration.ConfigRuntimeMode);
}
