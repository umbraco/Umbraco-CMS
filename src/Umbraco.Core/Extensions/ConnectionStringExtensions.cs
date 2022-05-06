// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions;

public static class ConnectionStringExtensions
{
    public static bool IsConnectionStringConfigured(this ConnectionStrings connectionString)
        => connectionString != null &&
           !string.IsNullOrWhiteSpace(connectionString.ConnectionString) &&
           !string.IsNullOrWhiteSpace(connectionString.ProviderName);

    /// <summary>
    ///     Gets a connection string from configuration with placeholders replaced.
    /// </summary>
    public static string? GetUmbracoConnectionString(
        this IConfiguration configuration,
        string connectionStringName = Constants.System.UmbracoConnectionName) =>
        configuration.GetConnectionString(connectionStringName).ReplaceDataDirectoryPlaceholder();

    /// <summary>
    ///     Replaces instances of the |DataDirectory| placeholder in a string with the value of AppDomain DataDirectory.
    /// </summary>
    public static string? ReplaceDataDirectoryPlaceholder(this string input)
    {
        var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
        return input?.Replace(ConnectionStrings.DataDirectoryPlaceholder, dataDirectory);
    }
}
