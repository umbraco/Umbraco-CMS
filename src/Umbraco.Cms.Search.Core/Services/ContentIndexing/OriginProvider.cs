namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Default implementation of <see cref="IOriginProvider"/>. Generates a random origin identifier once per process.
/// </summary>
internal sealed class OriginProvider : IOriginProvider
{
    private static readonly string _origin = Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    public string GetCurrent() => _origin;
}
