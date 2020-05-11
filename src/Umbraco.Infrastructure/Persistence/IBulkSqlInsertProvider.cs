using System.Collections.Generic;

namespace Umbraco.Core.Persistence
{
    public interface IBulkSqlInsertProvider
    {
        string ProviderName { get; }
        int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records);
    }
}
