using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDictionaryRepository : IReadWriteQueryRepository<int, IDictionaryItem>
{
    IDictionaryItem? Get(Guid uniqueId);

    IEnumerable<IDictionaryItem> GetMany(params Guid[] uniqueIds) => Array.Empty<IDictionaryItem>();

    IEnumerable<IDictionaryItem> GetManyByKeys(params string[] keys) => Array.Empty<IDictionaryItem>();

    IDictionaryItem? Get(string key);

    IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId);

    Dictionary<string, Guid> GetDictionaryItemKeyMap();
}
