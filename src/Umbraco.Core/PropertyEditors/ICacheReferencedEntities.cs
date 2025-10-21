namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Optionally implemented by property editors, this defines a contract for caching entities that are referenced in block values.
/// </summary>
public interface ICacheReferencedEntities
{
    /// <summary>
    /// Caches the entities referenced by the provided block data values.
    /// </summary>
    /// <param name="values">An enumerable collection of block values that may contain the entities to be cached.</param>
    void CacheReferencedEntities(IEnumerable<object> values);
}
