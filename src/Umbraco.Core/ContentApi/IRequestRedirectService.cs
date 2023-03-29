using Umbraco.Cms.Core.Models.ContentApi;

namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestRedirectService
{
    /// <summary>
    ///     Retrieves the redirect URL (if any) for a requested content path
    /// </summary>
    IApiContentRoute? GetRedirectPath(string requestedPath);
}
