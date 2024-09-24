namespace Umbraco.Cms.Infrastructure.HybridCache;

public interface ISeedKeyProvider
{
    /// <summary>
    /// Gets keys of documents that should be seeded into the cache.
    /// </summary>
    /// <returns>Keys to seed</returns>
    ISet<Guid> GetSeedKeys();
}
