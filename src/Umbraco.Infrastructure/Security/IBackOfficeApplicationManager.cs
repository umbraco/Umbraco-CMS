namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// Represents a contract for managing the state and operations of back office applications in Umbraco.
/// </summary>
public interface IBackOfficeApplicationManager
{
    /// <summary>
    /// Ensures that the back office application is initialized and configured for the specified hosts.
    /// </summary>
    /// <param name="backOfficeHosts">A collection of URIs representing the back office hosts to initialize.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EnsureBackOfficeApplicationAsync(IEnumerable<Uri> backOfficeHosts, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    Task EnsureBackOfficeClientCredentialsApplicationAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a back office client credentials application by its client ID.
    /// </summary>
    /// <param name="clientId">The client ID of the application to delete.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteBackOfficeClientCredentialsApplicationAsync(string clientId, CancellationToken cancellationToken = default);
}
