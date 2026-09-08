using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
    ///     The connection string keywords Umbraco has historically treated as also setting the command timeout.
    /// </summary>
    // TODO (V19): remove, along with the connect timeout fallback it exists to support.
    private static readonly string[] _deprecatedConnectTimeoutKeywords = ["connection timeout", "connect timeout"];

    private static readonly ConcurrentDictionary<DbProviderFactory, int?> _providerDefaults = new();

    /// <summary>
    ///     Gets the command timeout, in seconds, that the provider will apply to commands created from the
    ///     given connection string.
    /// </summary>
    /// <param name="provider">The provider factory, or <c>null</c> when unknown.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    ///     The timeout in seconds, where zero means no limit, or <c>null</c> when it cannot be determined.
    /// </returns>
    internal static int? GetConfiguredCommandTimeout(DbProviderFactory? provider, string? connectionString)
    {
        if (provider is null || string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        // Deliberately not memoized per connection string: callers already resolve this at most once
        // each, and caching it would hold credentials in a static for the lifetime of the process.
        return Probe(provider, connectionString);
    }

    /// <summary>
    ///     Determines whether the connection string relies on the deprecated convention of taking the command
    ///     timeout from the connect timeout, which is the case when it sets a connect timeout and does not
    ///     appear to configure a command timeout.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         "Does not appear to" is the accurate phrasing: a configured command timeout is detected by
    ///         comparing what the provider derives from this connection string against what it derives from no
    ///         connection string at all. A command timeout explicitly set to the provider's own default is
    ///         therefore indistinguishable from one that is absent, and the connect timeout wins.
    ///     </para>
    ///     <para>
    ///         That is the conservative choice, because it is what every version before the command timeout was
    ///         honoured did. Resolving the ambiguity would mean asking each provider which keywords the
    ///         connection string actually supplied, which is not something the ADO.NET contract exposes.
    ///     </para>
    /// </remarks>
    /// <param name="provider">The provider factory, or <c>null</c> when unknown.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="timeoutSeconds">The connect timeout to apply as the command timeout.</param>
    /// <param name="keyword">The connection string keyword the timeout was read from.</param>
    /// <returns><c>true</c> when the fallback applies; otherwise, <c>false</c>.</returns>
    [Obsolete("Deriving the command timeout from the connect timeout is deprecated. Scheduled for removal in Umbraco 19.")]
    internal static bool TryGetDeprecatedConnectTimeout(
        DbProviderFactory? provider,
        string? connectionString,
        out int timeoutSeconds,
        [NotNullWhen(true)] out string? keyword)
    {
        timeoutSeconds = 0;
        keyword = null;

        if (TryGetConnectTimeout(connectionString, out timeoutSeconds, out keyword) is false)
        {
            return false;
        }

        int? configuredCommandTimeout = GetConfiguredCommandTimeout(provider, connectionString);
        int? providerDefault = GetProviderDefaultCommandTimeout(provider);

        // A command timeout in the connection string takes precedence, so the fallback stands down. When
        // neither value could be determined we cannot tell the two apart, and preserving the long-standing
        // behaviour is the safer of the two options.
        if (configuredCommandTimeout is not null && providerDefault is not null && configuredCommandTimeout != providerDefault)
        {
            keyword = null;
            timeoutSeconds = 0;
            return false;
        }

        return true;
    }

    private static bool TryGetConnectTimeout(
        string? connectionString,
        out int timeoutSeconds,
        [NotNullWhen(true)] out string? keyword)
    {
        timeoutSeconds = 0;
        keyword = null;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        DbConnectionStringBuilder connectionStringParser;
        try
        {
            connectionStringParser = new DbConnectionStringBuilder { ConnectionString = connectionString };
        }
        catch (ArgumentException)
        {
            return false;
        }

        foreach (var candidate in _deprecatedConnectTimeoutKeywords)
        {
            if (connectionStringParser.TryGetValue(candidate, out var value)
                && int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                && parsed > 0)
            {
                timeoutSeconds = parsed;
                keyword = candidate;
                return true;
            }
        }

        return false;
    }

    private static int? GetProviderDefaultCommandTimeout(DbProviderFactory? provider)
    {
        if (provider is null)
        {
            return null;
        }

        return _providerDefaults.GetOrAdd(provider, static key => Probe(key, connectionString: null));
    }

    private static int? Probe(DbProviderFactory provider, string? connectionString)
    {
        try
        {
            using DbConnection? connection = provider.CreateConnection();
            if (connection is null)
            {
                return null;
            }

            if (connectionString is not null)
            {
                connection.ConnectionString = connectionString;
            }

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
