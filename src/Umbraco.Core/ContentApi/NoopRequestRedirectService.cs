namespace Umbraco.Cms.Core.ContentApi;

public sealed class NoopRequestRedirectService : IRequestRedirectService
{
    /// <inheritdoc />
    public string? GetRedirectPath(string requestedPath) => null;
}
