// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for a connection string.
/// </summary>
public static class ConnectionStringExtensions
{
    /// <summary>
    /// Determines whether the connection string is configured (set to a non-empty value).
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    ///   <c>true</c> if the connection string is configured; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsConnectionStringConfigured(this ConnectionStrings connectionString)
        => connectionString != null &&
        !string.IsNullOrWhiteSpace(connectionString.ConnectionString) &&
        !string.IsNullOrWhiteSpace(connectionString.ProviderName);
}
