namespace Umbraco.Cms.ManagementApi.Authorization;

public interface IBackOfficeApplicationManager
{
    Task EnsureBackOfficeApplicationAsync(Uri url, CancellationToken cancellationToken = default);
}
