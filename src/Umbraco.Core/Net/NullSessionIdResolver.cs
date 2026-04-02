namespace Umbraco.Cms.Core.Net;

/// <summary>
/// A null implementation of <see cref="ISessionIdResolver"/> that always returns <c>null</c>.
/// </summary>
/// <remarks>
/// This implementation is used when session support is not available or not configured.
/// </remarks>
public class NullSessionIdResolver : ISessionIdResolver
{
    /// <inheritdoc />
    public string? SessionId => null;
}
