namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Extension methods for expressing the timeout a distributed lock waits for as a database
///     command timeout.
/// </summary>
public static class DistributedLockTimeoutExtensions
{
    private const int MarginInSeconds = 5;

    /// <summary>
    ///     Converts the timeout a distributed lock waits for to the whole number of seconds that the
    ///     statement obtaining it should run under.
    /// </summary>
    /// <param name="lockTimeout">The timeout the lock waits for.</param>
    /// <returns>The command timeout in whole seconds.</returns>
    /// <remarks>
    ///     Allows a margin on top of the lock timeout, so that a mechanism able to report a lock timeout
    ///     of its own gets to do so before the client abandons the command. Rounds up, so that a
    ///     sub-second timeout cannot become zero seconds - which providers read as no limit at all.
    /// </remarks>
    public static int ToLockCommandTimeoutSeconds(this TimeSpan lockTimeout)
        => (int)Math.Ceiling(lockTimeout.TotalSeconds) + MarginInSeconds;
}
