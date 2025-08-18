namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Checks if a configured database is available on boot.
/// </summary>
public interface IDatabaseAvailabilityCheck
{
    /// <summary>
    /// Checks if the database is available for Umbraco to boot.
    /// </summary>
    /// <param name="databaseFactory">The <see cref="IUmbracoDatabaseFactory"/>.</param>
    /// <returns>
    /// A value indicating whether the database is available.
    /// </returns>
    bool IsDatabaseAvailable(IUmbracoDatabaseFactory databaseFactory);
}
