namespace Umbraco.Cms.Infrastructure.Security;

public interface IBackOfficeApplicationManager
{
    Task EnsureBackOfficeApplicationAsync(Uri backOfficeUrl, CancellationToken cancellationToken = default);

    Task EnsureBackOfficeClientCredentialsApplicationAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);

    Task DeleteBackOfficeClientCredentialsApplicationAsync(string clientId, CancellationToken cancellationToken = default);
}
