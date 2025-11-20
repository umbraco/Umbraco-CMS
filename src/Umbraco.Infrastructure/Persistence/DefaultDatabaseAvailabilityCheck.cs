using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Checks if a configured database is available on boot using the default method of 5 attempts with a 1 second delay between each one.
/// </summary>
internal class DefaultDatabaseAvailabilityCheck : IDatabaseAvailabilityCheck
{
    private const int NumberOfAttempts = 5;
    private const int DefaultAttemptDelayMilliseconds = 1000;

    private readonly ILogger<DefaultDatabaseAvailabilityCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDatabaseAvailabilityCheck"/> class.
    /// </summary>
    /// <param name="logger"></param>
    public DefaultDatabaseAvailabilityCheck(ILogger<DefaultDatabaseAvailabilityCheck> logger) => _logger = logger;

    /// <summary>
    /// Gets or sets the number of milliseconds to delay between attempts.
    /// </summary>
    /// <remarks>
    /// Exposed for testing purposes, hence settable only internally.
    /// </remarks>
    public int AttemptDelayMilliseconds { get; internal set; } = DefaultAttemptDelayMilliseconds;

    /// <inheritdoc/>
    public bool IsDatabaseAvailable(IUmbracoDatabaseFactory databaseFactory)
    {
        bool canConnect;
        for (var i = 0; ;)
        {
            canConnect = databaseFactory.CanConnect;
            if (canConnect || ++i == NumberOfAttempts)
            {
                break;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Could not immediately connect to database, trying again.");
            }

            // Wait for the configured time before trying again.
            Thread.Sleep(AttemptDelayMilliseconds);
        }

        return canConnect;
    }
}
