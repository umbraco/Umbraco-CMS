using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDictionaryRepository : IReadWriteQueryRepository<int, IDictionaryItem>
{
    IDictionaryItem? Get(Guid uniqueId);
    IDictionaryItem? Get(string key);
    IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId);
    Dictionary<string, Guid> GetDictionaryItemKeyMap();
}
