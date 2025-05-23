namespace Umbraco.Cms.Infrastructure.Security;

public interface IMemberApplicationManager
{
    Task EnsureMemberApplicationAsync(IEnumerable<Uri> loginRedirectUrls, IEnumerable<Uri> logoutRedirectUrls, CancellationToken cancellationToken = default);

    Task DeleteMemberApplicationAsync(CancellationToken cancellationToken = default);

    Task EnsureMemberClientCredentialsApplicationAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    Task DeleteMemberClientCredentialsApplicationAsync(string clientId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
