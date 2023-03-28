namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestRedirectService
{
    /// <summary>
    ///     Retrieves the redirect URL (if any) for a requested content path
    /// </summary>
    string? GetRedirectPath(string requestedPath);
}
