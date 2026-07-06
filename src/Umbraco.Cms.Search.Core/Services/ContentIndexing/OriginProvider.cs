namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal sealed class OriginProvider : IOriginProvider
{
    private static readonly string _origin = Guid.NewGuid().ToString("N");

    public string GetCurrent() => _origin;
}
