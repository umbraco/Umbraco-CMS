using System.Data.Common;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Resolves the command timeout that applies to a database connection string.
/// </summary>
/// <remarks>
///     <para>
///         The command timeout is a concept the ADO.NET provider owns: it decides which connection string
///         keywords express one, what they are called, and what the default is when none is given. Rather
///         than model that vocabulary, this type asks the provider directly, by building a command from an
///         unopened connection and reading the timeout the provider put on it.
///     </para>
/// </remarks>
internal static class CommandTimeoutResolver
{
    /// <summary>
    ///     Gets the command timeout, in seconds, that the provider will apply to commands created from the
    ///     given connection string.
    /// </summary>
    /// <param name="provider">The provider factory, or <c>null</c> when unknown.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    ///     The timeout in seconds, where zero means no limit, or <c>null</c> when it cannot be determined.
    /// </returns>
    /// <remarks>
    ///     Deliberately not memoized: callers resolve this at most once each, and caching it would hold
    ///     credentials in a static for the lifetime of the process.
    /// </remarks>
    internal static int? GetConfiguredCommandTimeout(DbProviderFactory? provider, string? connectionString)
    {
        if (provider is null || string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        try
        {
            using DbConnection? connection = provider.CreateConnection();
            if (connection is null)
            {
                return null;
            }

            connection.ConnectionString = connectionString;

            using DbCommand command = connection.CreateCommand();
            return command.CommandTimeout;
        }
        catch (Exception)
        {
            // A connection string the provider rejects has to surface as the connection failure it is, when a
            // connection is actually opened - not as a failure to construct a database.
            return null;
        }
    }
}
