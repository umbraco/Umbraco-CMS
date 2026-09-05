namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Extension methods for expressing a configured timeout in a database connection string.
/// </summary>
public static class ConnectionStringTimeoutExtensions
{
    /// <summary>
    ///     Converts a configured timeout to the whole number of seconds that a connection string
    ///     timeout keyword takes.
    /// </summary>
    /// <param name="timeout">The configured timeout.</param>
    /// <returns>The timeout in whole seconds.</returns>
    /// <remarks>
    ///     Rounds up, so that a sub-second timeout cannot become zero seconds - which providers read as
    ///     no limit at all, the opposite of what was configured.
    /// </remarks>
    public static int ToConnectionStringTimeoutSeconds(this TimeSpan timeout)
        => (int)Math.Ceiling(timeout.TotalSeconds);
}
