using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDictionaryRepository : IRepositoryQueryable<int, IDictionaryItem>
    {
        IDictionaryItem Get(Guid uniqueId);
        IDictionaryItem Get(string key);
        IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId);
    }
}