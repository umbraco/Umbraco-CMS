namespace Umbraco.Cms.Infrastructure.Security;

public interface IBackOfficeApplicationManager
{
    Task EnsureBackOfficeApplicationAsync(IEnumerable<Uri> backOfficeHosts, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    Task EnsureBackOfficeClientCredentialsApplicationAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);

    Task DeleteBackOfficeClientCredentialsApplicationAsync(string clientId, CancellationToken cancellationToken = default);
}
