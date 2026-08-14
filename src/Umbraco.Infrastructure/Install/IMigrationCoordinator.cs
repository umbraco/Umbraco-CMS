namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
/// Coordinates migration leadership across servers in a load-balanced environment.
/// </summary>
internal interface IMigrationCoordinator
{
    /// <summary>
    /// Attempts to become the migration leader, blocking until either this server wins the claim
    /// or another server completes all migrations.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the leadership wait loop.</param>
    /// <returns>
    /// <c>true</c> if this server is the migration leader and must call <see cref="ReleaseLeadership"/> after
    /// running migrations; <c>false</c> if another server completed migrations and this server should skip them.
    /// </returns>
    Task<bool> TryBecomeLeaderAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Releases the migration leadership claim if it is still held by this instance.
    /// Must be called in a <c>finally</c> block to ensure release even on failure.
    /// </summary>
    void ReleaseLeadership();
}
