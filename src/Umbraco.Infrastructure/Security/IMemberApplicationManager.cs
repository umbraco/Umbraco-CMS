namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// Represents a contract for managing member applications in the Umbraco CMS infrastructure.
/// </summary>
public interface IMemberApplicationManager
{
    /// <summary>
    /// Ensures the member application is configured with the specified login and logout redirect URLs.
    /// </summary>
    /// <param name="loginRedirectUrls">The collection of URIs to redirect to after login.</param>
    /// <param name="logoutRedirectUrls">The collection of URIs to redirect to after logout.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureMemberApplicationAsync(IEnumerable<Uri> loginRedirectUrls, IEnumerable<Uri> logoutRedirectUrls, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes the member application.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous delete operation.</returns>
    Task DeleteMemberApplicationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that a member client credentials application exists for the specified client ID and secret, creating it if necessary.
    /// </summary>
    /// <param name="clientId">The client ID for the member application.</param>
    /// <param name="clientSecret">The client secret associated with the client ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous ensure operation.</returns>
    Task EnsureMemberClientCredentialsApplicationAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Asynchronously deletes the member client credentials application associated with the specified client ID.
    /// </summary>
    /// <param name="clientId">The client ID of the application to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteMemberClientCredentialsApplicationAsync(string clientId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
