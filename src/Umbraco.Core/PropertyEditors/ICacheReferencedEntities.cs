namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Optionally implemented by property editors, this defines a contract for caching entities that are referenced in block values.
/// </summary>
[Obsolete("This interface is available for support of request caching retrieved entities in property value editors that implement it. " +
          "The intention is to supersede this with lazy loaded read locks, which will make this unnecessary. " +
          "Scheduled for removal in Umbraco 19.")]
public interface ICacheReferencedEntities
{
    /// <summary>
    /// Caches the entities referenced by the provided block data values.
    /// </summary>
    /// <param name="values">An enumerable collection of block values that may contain the entities to be cached.</param>
    [Obsolete("This method is available for support of request caching retrieved entities in derived property value editors. " +
              "The intention is to supersede this with lazy loaded read locks, which will make this unnecessary. " +
              "Scheduled for removal in Umbraco 19.")]
    void CacheReferencedEntities(IEnumerable<object> values);
}
