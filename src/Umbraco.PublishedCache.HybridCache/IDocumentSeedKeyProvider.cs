namespace Umbraco.Cms.Infrastructure.HybridCache;

public interface IDocumentSeedKeyProvider
{
    /// <summary>
    /// Gets keys of documents that should be seeded into the cache.
    /// </summary>
    /// <returns>Keys to seed</returns>
    IEnumerable<Guid> GetSeedKeys();
}
