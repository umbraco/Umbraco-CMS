namespace Umbraco.Cms.Infrastructure.Security;

public interface IBackOfficeApplicationManager
{
    [Obsolete("Please use the overload that allows for multiple back-office hosts. Will be removed in V17.")]
    Task EnsureBackOfficeApplicationAsync(Uri backOfficeUrl, CancellationToken cancellationToken = default);

    Task EnsureBackOfficeApplicationAsync(IEnumerable<Uri> backOfficeHosts, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    Task EnsureBackOfficeClientCredentialsApplicationAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);

    Task DeleteBackOfficeClientCredentialsApplicationAsync(string clientId, CancellationToken cancellationToken = default);
}
