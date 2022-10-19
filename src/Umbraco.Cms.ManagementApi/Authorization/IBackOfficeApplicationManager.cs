namespace Umbraco.Cms.ManagementApi.Authorization;

public interface IBackOfficeApplicationManager
{
    Task EnsureBackOfficeApplicationAsync(Uri backOfficeUrl, CancellationToken cancellationToken = default);
}
