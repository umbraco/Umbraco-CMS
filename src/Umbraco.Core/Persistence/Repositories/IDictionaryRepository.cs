using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDictionaryRepository : IReadWriteQueryRepository<int, IDictionaryItem>
    {
        IDictionaryItem Get(Guid uniqueId);
        IDictionaryItem Get(string key);
        IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId);
        Dictionary<string, Guid> GetDictionaryItemKeyMap();
        IEnumerable<MoveEventInfo<IDictionaryItem>> Move(IDictionaryItem toMove, IDictionaryItem parent);
    }
}
