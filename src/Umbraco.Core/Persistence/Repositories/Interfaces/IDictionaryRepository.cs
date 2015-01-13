using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDictionaryRepository : IRepositoryQueryable<int, IDictionaryItem>
    {
        IDictionaryItem Get(Guid uniqueId);
        IDictionaryItem Get(string key);
    }
}